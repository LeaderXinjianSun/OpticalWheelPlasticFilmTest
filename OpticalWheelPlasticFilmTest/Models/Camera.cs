using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SXJ
{
    public class Camera
    {
        private HFramegrabber Framegrabber { set; get; }
        public string[] GetDevies(string cameraInterface = "GigEVision")
        {
            try
            {
                HTuple information, valueList;
                HOperatorSet.InfoFramegrabber(cameraInterface, "device",out information, out valueList);
                return valueList.SArr;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public bool OpenCamera(string cameraName, string CameraInterface = "GigEVision")
        {
            try
            {
                Framegrabber = new HFramegrabber(CameraInterface, 0, 0, 0, 0, 0, 0, "default", -1, "default", -1, "false", "default", cameraName, 0, -1);

                return true;
            }
            catch (HalconException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public void CloseCamera()
        {
            try
            {
                Framegrabber?.Dispose();
            }
            catch { }
        }
        public HImage GrabImage()
        {
            try
            {
                return Framegrabber.GrabImage();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        public HImage GrabEnhancedImage()
        {
            try
            {
                return new HImage(EnhancedImage(Framegrabber.GrabImage()));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        public static HObject EnhancedImage(HObject image)
        {
            HObject imageScaled;
            HTuple mult = 255.0 / (255 - 70);
            HTuple add = -1 * mult * 70;
            HOperatorSet.ScaleImage(image, out imageScaled, mult, add);
            return imageScaled;
        }
        public void GrabeImageStart()
        {
            Framegrabber.GrabImageStart(-1);

        }
        public HImage GrabeImageAsync()
        {
            try
            {
                return Framegrabber.GrabImageAsync(-1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
