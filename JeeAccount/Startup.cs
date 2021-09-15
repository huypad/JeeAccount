using DPSinfra.Kafka;
using DPSinfra.Logger;
using DPSinfra.Vault;
using JeeAccount.ControllersKafka;
using JeeAccount.Reponsitories;
using JeeAccount.Reponsitories.CustomerManagement;
using JeeAccount.Reponsitories.DatabaseManagement;
using JeeAccount.Reponsitories.JobtitleManagement;
using JeeAccount.Reponsitories.Mail;
using JeeAccount.Reponsitories.PermissionManagement;
using JeeAccount.Services.AccountManagementService;
using JeeAccount.Services.CommentService;
using JeeAccount.Services.CustomerManagementService;
using JeeAccount.Services.DatabaseManagementService;
using JeeAccount.Services.DepartmentManagement;
using JeeAccount.Services.JobtitleManagementService;
using JeeAccount.Services.MailService;
using JeeAccount.Services.PermissionManagementService;
using JeeAccount.Services.StructureManagementService;
using JeeCustomer.ConsumerKafka;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VaultSharp;
using VaultSharp.V1.Commons;

namespace JeeAccount
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // add Vault and get Vault for secret in another services
            var vaultClient = ConfigVault(services);

            Secret<SecretData> kafkaSecret = vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(path: "jwt", mountPoint: "kv").Result;
            IDictionary<string, object> kafkaDataSecret = kafkaSecret.Data.Data;
            string access_secret = kafkaDataSecret["access_secret"].ToString();
            string internal_secret = kafkaDataSecret["internal_secret"].ToString();
            Configuration["Jwt:access_secret"] = access_secret;
            Configuration["Jwt:internal_secret"] = internal_secret;
            Secret<SecretData> kafka = vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(path: "kafka", mountPoint: "kv").Result;
            IDictionary<string, object> kafkaData = kafka.Data.Data;
            string KafkaUser = kafkaData["username"].ToString();
            string KafkaPassword = kafkaData["password"].ToString();
            Configuration["KafkaConfig:username"] = KafkaUser;
            Configuration["KafkaConfig:password"] = KafkaPassword;
            Secret<SecretData> minioSecret = vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(path: "minio", mountPoint: "kv").Result;
            IDictionary<string, object> minioData = minioSecret.Data.Data;
            Configuration["MinioConfig:MinioAccessKey"] = minioData["access_key"].ToString();
            Configuration["MinioConfig:MinioSecretKey"] = minioData["secret_key"].ToString();
            System.Console.WriteLine(Configuration["MinioConfig:MinioAccessKey"]);
            System.Console.WriteLine(Configuration["MinioConfig:MinioSecretKey"]);
            // add Kafka
            services.addKafkaService();
            services.AddCors(o => o.AddPolicy("AllowOrigin", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            services.AddControllers().AddNewtonsoftJson();
            services.AddControllersWithViews().AddNewtonsoftJson();
            services.AddRazorPages().AddNewtonsoftJson();
            services.AddControllers().AddNewtonsoftJson(options => { options.SerializerSettings.ContractResolver = new DefaultContractResolver(); });

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    //ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(access_secret)),
                };
            });

            //Swagger
            services.AddSwaggerGen(c =>
            {
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Please enter into field the word 'Bearer' following by space and JWT",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    //Scheme = "bearer", // must be lower case
                    Reference = new OpenApiReference
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme
                    }
                };
                c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                                        {
                                            {securityScheme, new string[] { }}
                                        });
            });

            services.AddMvc().ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddOptions();

            services.AddLogging(builder =>
            {
                builder.addAsyncLogger<AsyncLoggerProvider>(p => new AsyncLoggerProvider(p.GetService<IProducer>()));
            });

            #region add kafka consumer

            services.AddSingleton<IHostedService, AccountConsumerController>();
            services.AddSingleton<IHostedService, PermissionConsumerController>();

            #endregion add kafka consumer

            #region add Repository

            services.AddTransient<IAccountManagementReponsitory, AccountManagementReponsitory>();
            services.AddTransient<ICustomerManagementReponsitory, CustomerManagementReponsitory>();
            services.AddTransient<IDepartmentManagementReponsitory, DepartmentManagementReponsitory>();
            services.AddSingleton<IDatabaseManagementRepositoty, DatabaseManagementRepositoty>();
            services.AddSingleton<IMailReponsitory, MailReponsitory>();
            services.AddSingleton<IStructureManagementReponsitory, StructureManagementReponsitory>();
            services.AddSingleton<IWidgetDashBoardRepository, WidgetDashBoardRepository>();
            services.AddTransient<IJobtitleManagementReponsitory, JobtitleManagementReponsitory>();
            services.AddTransient<IPermissionManagementRepository, PermissionManagementRepository>();

            #endregion add Repository

            #region add Services

            services.AddTransient<IAccountManagementService, AccountManagementService>();
            services.AddTransient<IDepartmentManagementService, DepartmentManagementService>();
            services.AddTransient<ICustomerManagementService, CustomerManagementService>();
            services.AddSingleton<IDatabaseManagementService, DatabaseManagementService>();
            services.AddSingleton<IMailService, MailService>();
            services.AddSingleton<IStructureManagementService, StructureManagementService>();
            services.AddSingleton<ICommentService, CommentService>();
            services.AddTransient<IJobtitleManagementService, JobtitleManagementService>();
            services.AddTransient<IPermissionManagementService, PermissionManagementService>();

            #endregion add Services
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "JeeAccount v1"));
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("AllowOrigin");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStaticFiles();// For the wwwroot folder

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                            Path.Combine(Directory.GetCurrentDirectory(), "images")),
                RequestPath = "/images"
            });
            //Enable directory browsing
            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = new PhysicalFileProvider(
                            Path.Combine(Directory.GetCurrentDirectory(), "images")),
                RequestPath = "/images"
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private VaultClient ConfigVault(IServiceCollection services)
        {
            var serviceConfig = Configuration.GetVaultConfig();
            return services.addVaultService(serviceConfig);
        }
    }
}