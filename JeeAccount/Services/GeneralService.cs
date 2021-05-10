using JeeAccount.Classes;
using JeeAccount.Models;
using JeeAccount.Models.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Services
{
    public class GeneralService
    {
        public static string CreateListStringWhereIn(List<string> ListStringData)
        {
            string result = "";
            foreach(string data in ListStringData)
            {
                if (string.IsNullOrEmpty(result))
                {
                    result = $"'{data}'";
                }
                else
                {
                    result += $", '{data}'";
                }
            }
            return result;
        }

        public static Aspose.Imaging.Image RezizeImage(Aspose.Imaging.Image img, int maxHeight)
        {
            if (img.Height < maxHeight) return img;
            Double yRatio = (double)img.Height / maxHeight;
            Double ratio = yRatio;
            int nnx = (int)Math.Floor(img.Width / ratio);
            int nny = (int)Math.Floor(img.Height / ratio);
            img.Resize(nnx, nny);
            return img;
        }

        public static void saveImgNhanVien(string image, string userID, long customerID)
        {

            byte[] ImageArray = Convert.FromBase64String(image);

            string ID_NV = userID;

            String pathOriginal = Environment.CurrentDirectory + "/images/nhanvien/" + customerID + "/Original/";


            //Check if directory exist
            if (!System.IO.Directory.Exists(pathOriginal))
            {
                System.IO.Directory.CreateDirectory(pathOriginal); //Create directory if it doesn't exist
            }

            string imageNameOriginal = ID_NV + ".jpg";

            //set the image path
            string imgPathOriginal = Path.Combine(pathOriginal, imageNameOriginal);

            if (System.IO.File.Exists(imgPathOriginal))
            {
                System.IO.File.Delete(imgPathOriginal);
            };

            System.IO.File.WriteAllBytes(imgPathOriginal, ImageArray);

            using (Aspose.Imaging.Image myImg = Aspose.Imaging.Image.Load(imgPathOriginal))
            {
                RezizeImage(myImg, 130);
                String path = Environment.CurrentDirectory + "/images/nhanvien/" + customerID + "/";

                //Check if directory exist
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path); //Create directory if it doesn't exist
                }

                string imageName = ID_NV + ".jpg";

                //set the image path
                string imgPath = Path.Combine(path, imageName);

                //Delete if file exist
                if (System.IO.File.Exists(imgPath))
                {
                    System.IO.File.Delete(imgPath);
                };

                myImg.Save(imgPath);
            }

        }

        public static string getlastname(string fullname)
        {
            if (fullname.Contains(' '))
            {
                string[] word = fullname.Split(' ');
                string lastName = word[0];
                for (var index = 1; index < word.Length - 1; index++)
                {
                    lastName += " " + word[index];
                }
                return lastName;
            }
            return fullname;
        }
        public static string getFirstname(string fullname)
        {
            if (fullname.Contains(' '))
            {
                string[] word = fullname.Split(' ');
                string firstName = word[word.Length-1];
                return firstName;
            }
            return fullname;
        }

        public static IdentityServerReturn TranformIdentityServerReturnSqlModel(ReturnSqlModel returnSql)
        {
            IdentityServerReturn identity = new IdentityServerReturn();
            identity.statusCode = Int32.Parse(returnSql.ErrorCode);
            identity.message = returnSql.ErrorMessgage;
            return identity;
        }

        public static IdentityServerReturn TranformIdentityServerException(Exception ex)
        {
            IdentityServerReturn identity = new IdentityServerReturn();
            identity.statusCode = Int32.Parse(Constant.ERRORCODE_EXCEPTION);
            return identity;
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghjklmnoprstuwxyz@!@!@!";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

}
