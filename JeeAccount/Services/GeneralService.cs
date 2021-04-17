using JeeAccount.Models;
using JeeAccount.Models.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        public static System.Drawing.Image RezizeImage(System.Drawing.Image img, int maxHeight)
        {
            if (img.Height < maxHeight) return img;
            using (img)
            {
                Double yRatio = (double)img.Height / maxHeight;
                Double ratio = yRatio;
                int nnx = (int)Math.Floor(img.Width / ratio);
                int nny = (int)Math.Floor(img.Height / ratio);
                Bitmap cpy = new Bitmap(nnx, nny, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics gr = Graphics.FromImage(cpy))
                {
                    gr.Clear(System.Drawing.Color.Transparent);

                    // This is said to give best quality when resizing images
                    gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                    gr.DrawImage(img,
                        new Rectangle(0, 0, nnx, nny),
                        new Rectangle(0, 0, img.Width, img.Height),
                        GraphicsUnit.Pixel);
                }
                return cpy;
            }
        }

        public static MemoryStream BytearrayToStream(byte[] arr)
        {
            return new MemoryStream(arr, 0, arr.Length);
        }

        public static byte[] CopyImageToByteArray(System.Drawing.Image theImage)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                theImage.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                return memoryStream.ToArray();
            }
        }

        public static void saveImgNhanVien(string image, string userID, long customerID)
        {

            byte[] ImageArray = Convert.FromBase64String(image);
            System.Drawing.Image myImg = RezizeImage(System.Drawing.Image.FromStream(GeneralService.BytearrayToStream(ImageArray)), 130);

            byte[] AvatarImageArray = CopyImageToByteArray(myImg);

            string ID_NV = userID;

            String path = Environment.CurrentDirectory + "\\images\\nhanvien\\" + customerID + "\\";

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

            System.IO.File.WriteAllBytes(imgPath, AvatarImageArray);

            String pathOriginal = Environment.CurrentDirectory + "\\images\\nhanvien\\" + customerID + "\\Original\\";


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

        public static IdentityServerReturn TranformIdentityServerReturnToRe(ReturnSqlModel returnSql)
        {
            IdentityServerReturn identity = new IdentityServerReturn();
            identity.statusCode = Int32.Parse(returnSql.ErrorCode);
            identity.message = returnSql.ErrorMessgage;
            return identity;
        }


    }

}
