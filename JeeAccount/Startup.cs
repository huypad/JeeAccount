using DPSinfra.Kafka;
using DPSinfra.Logger;
using DPSinfra.Vault;
using JeeAccount.Reponsitories;
using JeeAccount.Reponsitories.CustomerManagement;
using JeeAccount.Reponsitories.DatabaseManagement;
using JeeAccount.Reponsitories.Mail;
using JeeAccount.Services.AccountManagementService;
using JeeAccount.Services.CommentService;
using JeeAccount.Services.CustomerManagementService;
using JeeAccount.Services.DatabaseManagementService;
using JeeAccount.Services.DepartmentManagementService;
using JeeAccount.Services.MailService;
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
                //options.RequireHttpsMetadata = false;
                //options.SaveToken = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(access_secret.ToString())),
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

            #region add Repository

            services.AddTransient<IAccountManagementReponsitory, AccountManagementReponsitory>();
            services.AddTransient<ICustomerManagementReponsitory, CustomerManagementReponsitory>();
            services.AddTransient<IDatabaseManagementRepositoty, DatabaseManagementRepositoty>();
            services.AddTransient<IDepartmentManagementReponsitory, DepartmentManagementReponsitory>();
            services.AddTransient<IMailReponsitory, MailReponsitory>();
            services.AddTransient<IStructureManagementReponsitory, StructureManagementReponsitory>();
            services.AddTransient<IWidgetDashBoardRepository, WidgetDashBoardRepository>();

            #endregion add Repository

            #region add Services

            services.AddTransient<IAccountManagementService, AccountManagementService>();
            services.AddTransient<ICustomerManagementService, CustomerManagementService>();
            services.AddTransient<IDatabaseManagementService, DatabaseManagementService>();
            services.AddTransient<IDepartmentManagementService, DepartmentManagementService>();
            services.AddTransient<IMailService, MailService>();
            services.AddTransient<IStructureManagementService, StructureManagementService>();
            services.AddTransient<ICommentService, CommentService>();

            #endregion add Services

            #region add kafka consumer

            services.AddSingleton<IHostedService, AccountConsumerController>();

            #endregion add kafka consumer
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