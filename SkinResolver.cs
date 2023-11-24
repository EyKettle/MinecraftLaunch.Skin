using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;

namespace MinecraftLaunch.Skin {
    public class SkinResolver {
        private byte[] ImageBytes = null!;

        public SkinResolver(string url) {
            ImageBytes = url.GetBytesAsync().Result;
        }

        public SkinResolver(byte[] bytes) {
            ImageBytes = bytes;
        }

        public SkinResolver(FileInfo filePath) {
            if (filePath.Exists) {
                ImageBytes = File.ReadAllBytes(filePath.FullName);
            } else {
                throw new FileNotFoundException();
            }
        }

        /// <summary>
        /// 裁剪皮肤图片头像
        /// </summary>
        /// <returns>裁剪后图片</returns>
        public Image<Rgba32> CropSkinHeadBitmap() {
            Image<Rgba32> head = (Image<Rgba32>)Image.Load(ImageBytes);
            head.Mutate(x => x.Crop(Rectangle.FromLTRB(8, 8, 16, 16)));

            Image<Rgba32> hat = (Image<Rgba32>)Image.Load(ImageBytes);
            hat.Mutate(x => x.Crop(Rectangle.FromLTRB(40, 8, 48, 16)));

            Image<Rgba32> endImage = new Image<Rgba32>(8, 8);
            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    endImage[i, j] = head[i, j];
                    if (hat[i, j].A == 255) {
                        endImage[i, j] = hat[i, j];
                    }
                }
            }

            return ResizeImage(endImage, 60, 60);
        }

        /// <summary>
        /// 裁剪皮肤图片身体
        /// </summary>
        /// <typeparam name="TPixel"></typeparam>
        /// <param name="skin"></param>
        /// <returns></returns>
        public Image<Rgba32> CropSkinBodyBitmap() {
            var skin = ImageHelper.ConvertToImage(ImageBytes);

            Image<Rgba32> Body = CopyBitmap(skin);
            Body.Mutate(x => x.Crop(Rectangle.FromLTRB(20, 20, 28, 32)));
            return ResizeImage(Body, 60, 90);
        }

        /// <summary>
        /// 裁剪皮肤图片右手
        /// </summary>
        /// <typeparam name="TPixel"></typeparam>
        /// <param name="skin"></param>
        /// <returns></returns>
        public Image<Rgba32> CropRightHandBitmap() {
            var skin = ImageHelper.ConvertToImage(ImageBytes);

            Image<Rgba32> Arm = CopyBitmap(skin);
            Arm.Mutate(x => x.Crop(Rectangle.FromLTRB(35, 52, 39, 64)));
            return ResizeImage(Arm, 30, 90);
        }

        /// <summary>
        /// 裁剪皮肤图片左手
        /// </summary>
        /// <typeparam name="TPixel"></typeparam>
        /// <param name="skin"></param>
        /// <returns></returns>
        public Image<Rgba32> CropLeftHandBitmap() {
            var skin = ImageHelper.ConvertToImage(ImageBytes);

            Image<Rgba32> Arm = CopyBitmap(skin);
            Arm.Mutate(x => x.Crop(Rectangle.FromLTRB(44, 20, 48, 32)));
            return ResizeImage(Arm, 30, 90);
        }

        /// <summary>
        /// 裁剪皮肤图片右腿
        /// </summary>
        /// <typeparam name="TPixel"></typeparam>
        /// <param name="skin"></param>
        /// <returns></returns>
        public Image<Rgba32> CropRightLegBitmap() {
            var skin = ImageHelper.ConvertToImage(ImageBytes);

            Image<Rgba32> Leg = CopyBitmap(skin);
            Leg.Mutate(x => x.Crop(Rectangle.FromLTRB(20, 52, 24, 64)));
            return ResizeImage(Leg, 30, 90);
        }

        /// <summary>
        /// 裁剪皮肤图片左腿
        /// </summary>
        /// <typeparam name="TPixel"></typeparam>
        /// <param name="skin"></param>
        /// <returns></returns>
        public Image<Rgba32> CropLeftLegBitmap() {
            var skin = ImageHelper.ConvertToImage(ImageBytes);

            Image<Rgba32> Leg = CopyBitmap(skin);
            Leg.Mutate(x => x.Crop(Rectangle.FromLTRB(4, 20, 8, 32)));
            return ResizeImage(Leg, 30, 90);
        }

        /// <summary>
        /// 重置图片长宽
        /// </summary>
        /// <typeparam name="TPixel"></typeparam>
        /// <param name="image"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public Image<Rgba32> ResizeImage(Image<Rgba32> image, int w, int h) {
            Image<Rgba32> image2 = new(w, h);
            for (int i = 0; i < w; i++) {
                for (int j = 0; j < h; j++) {
                    double tmp;
                    tmp = image.Width / (double)w;
                    double realW = tmp * (i);
                    tmp = image.Height / (double)h;
                    double realH = (tmp) * (j);
                    image2[i, j] = image[(int)realW, (int)realH];
                }
            }

            return image2;
        }

        /// <summary>
        /// 基础裁剪方法
        /// </summary>
        /// <typeparam name="TPixel"></typeparam>
        /// <param name="image"></param>
        /// <returns></returns>
        private Image<Rgba32> CopyBitmap(Image<Rgba32> image) {
            Image<Rgba32> tmp = new(image.Width, image.Height);
            for (int i = 0; i < image.Width; i++) {
                for (int j = 0; j < image.Height; j++) {
                    tmp[i, j] = image[i, j];
                }
            }

            return tmp;
        }

        public Image<Rgba32> CropDoubleSkinOverview(bool IsSlim = false, int ArmAngle = 8, int ArmShrinkPx = 6, int ScaleSize = 8)
        {
            var head = CropDoubleSkinHead(ScaleSize);
            var body = CropDoubleSkinBody(ScaleSize);
            var leftArm = CropDoubleSkinLeftArm(IsSlim, ScaleSize);
            var rightArm = CropDoubleSkinRightArm(IsSlim, ScaleSize);
            var leftLeg = CropDoubleSkinLeftLeg(ScaleSize);
            var rightLeg = CropDoubleSkinRightLeg(ScaleSize);

            Image<Rgba32> skin = new(264 * ScaleSize, 264 * ScaleSize);
            leftArm.Mutate(x => x.Rotate(-ArmAngle));
            rightArm.Mutate(x => x.Rotate(ArmAngle));
            skin.Mutate(x => x.DrawImage(leftLeg, new Point(128 * ScaleSize, 160 * ScaleSize), 1f));
            skin.Mutate(x => x.DrawImage(rightLeg, new Point(96 * ScaleSize, 160 * ScaleSize), 1f));
            skin.Mutate(x => x.DrawImage(leftArm, new Point((160 - ArmShrinkPx) * ScaleSize, 64 * ScaleSize), 1f));
            skin.Mutate(x => x.DrawImage(rightArm, new Point((IsSlim ? (104 + ArmShrinkPx) : (112 + ArmShrinkPx)) * ScaleSize - rightArm.Width, 64 * ScaleSize), 1f));
            skin.Mutate(x => x.DrawImage(body, new Point(96 * ScaleSize, 64 * ScaleSize), 1f));
            skin.Mutate(x => x.DrawImage(head, new Point(96 * ScaleSize, 0), 1f));
            head.Dispose();
            body.Dispose();
            leftArm.Dispose();
            rightArm.Dispose();
            leftLeg.Dispose();
            rightLeg.Dispose();

            return skin;
        }
        public Image<Rgba32> CropDoubleSkinHead(int ScaleSize = 8)
        {
            Image<Rgba32> innerHead = ImageHelper.ConvertToImage(ImageBytes);
            Image<Rgba32> outerHead = ImageHelper.ConvertToImage(ImageBytes);
            innerHead.Mutate(x => x.Crop(new(8, 8, 8, 8)));
            innerHead.Mutate(x => x.Resize(64, 64, KnownResamplers.NearestNeighbor));
            outerHead.Mutate(x => x.Crop(new(40, 8, 8, 8)));
            outerHead.Mutate(x => x.Resize(72, 72, KnownResamplers.NearestNeighbor));
            Image<Rgba32> head = new(72, 72);
            head.Mutate(x => x.DrawImage(innerHead, new Point(4, 4), 1f));
            head.Mutate(x => x.DrawImage(outerHead, new Point(0, 0), 1f));
            innerHead.Dispose();
            outerHead.Dispose();
            
            head.Mutate(x => x.Resize(head.Width * ScaleSize, head.Height * ScaleSize, KnownResamplers.NearestNeighbor));

            return head;
        }
        public Image<Rgba32> CropDoubleSkinBody(int ScaleSize = 8)
        {
            Image<Rgba32> innerBody = ImageHelper.ConvertToImage(ImageBytes);
            Image<Rgba32> outerBody = ImageHelper.ConvertToImage(ImageBytes);
            innerBody.Mutate(x => x.Crop(new(20, 20, 8, 12)));
            innerBody.Mutate(x => x.Resize(64, 96, KnownResamplers.NearestNeighbor));
            outerBody.Mutate(x => x.Crop(new(20, 36, 8, 12)));
            outerBody.Mutate(x => x.Resize(72, 104, KnownResamplers.NearestNeighbor));
            Image<Rgba32> body = new(72, 104);
            body.Mutate(x => x.DrawImage(innerBody, new Point(4, 4), 1f));
            body.Mutate(x => x.DrawImage(outerBody, new Point(0, 0), 1f));
            innerBody.Dispose();
            outerBody.Dispose();

            body.Mutate(x => x.Resize(body.Width * ScaleSize, body.Height * ScaleSize, KnownResamplers.NearestNeighbor));

            return body;
        }
        public Image<Rgba32> CropDoubleSkinLeftArm(bool IsSlim = false, int ScaleSize = 8)
        {
            Image<Rgba32> innerLeftArm = ImageHelper.ConvertToImage(ImageBytes);
            Image<Rgba32> outerLeftArm = ImageHelper.ConvertToImage(ImageBytes);
            innerLeftArm.Mutate(x => x.Crop(new(36, 52, IsSlim ? 3 : 4, 12)));
            outerLeftArm.Mutate(x => x.Crop(new(52, 52, IsSlim ? 3 : 4, 12)));
            innerLeftArm.Mutate(x => x.Resize(IsSlim ? 24 : 32, 96, KnownResamplers.NearestNeighbor));
            outerLeftArm.Mutate(x => x.Resize(IsSlim ? 32 : 40, 104, KnownResamplers.NearestNeighbor));
            Image<Rgba32> leftArm = new(IsSlim ? 32 : 40, 104);
            leftArm.Mutate(x => x.DrawImage(innerLeftArm, new Point(4, 4), 1f));
            leftArm.Mutate(x => x.DrawImage(outerLeftArm, new Point(0, 0), 1f));
            innerLeftArm.Dispose();
            outerLeftArm.Dispose();

            leftArm.Mutate(x => x.Resize(leftArm.Width * ScaleSize, leftArm.Height * ScaleSize, KnownResamplers.NearestNeighbor));

            return leftArm;
        }
        public Image<Rgba32> CropDoubleSkinRightArm(bool IsSlim = false, int ScaleSize = 8)
        {
            Image<Rgba32> innerRightArm = ImageHelper.ConvertToImage(ImageBytes);
            Image<Rgba32> outerRightArm = ImageHelper.ConvertToImage(ImageBytes);
            innerRightArm.Mutate(x => x.Crop(new(44, 20, IsSlim ? 3 : 4, 12)));
            outerRightArm.Mutate(x => x.Crop(new(44, 36, IsSlim ? 3 : 4, 12)));
            innerRightArm.Mutate(x => x.Resize(IsSlim ? 24 : 32, 96, KnownResamplers.NearestNeighbor));
            outerRightArm.Mutate(x => x.Resize(IsSlim ? 32 : 40, 104, KnownResamplers.NearestNeighbor));
            Image<Rgba32> rightArm = new(IsSlim ? 32 : 40, 104);
            rightArm.Mutate(x => x.DrawImage(innerRightArm, new Point(4, 4), 1f));
            rightArm.Mutate(x => x.DrawImage(outerRightArm, new Point(0, 0), 1f));
            innerRightArm.Dispose();
            outerRightArm.Dispose();

            rightArm.Mutate(x => x.Resize(rightArm.Width * ScaleSize, rightArm.Height * ScaleSize, KnownResamplers.NearestNeighbor));

            return rightArm;
        }
        public Image<Rgba32> CropDoubleSkinLeftLeg(int ScaleSize = 8)
        {
            Image<Rgba32> innerLeftLeg = ImageHelper.ConvertToImage(ImageBytes);
            Image<Rgba32> outerLeftLeg = ImageHelper.ConvertToImage(ImageBytes);
            innerLeftLeg.Mutate(x => x.Crop(new(20, 52, 4, 12)));
            outerLeftLeg.Mutate(x => x.Crop(new(4, 52, 4, 12)));
            innerLeftLeg.Mutate(x => x.Resize(32, 96, KnownResamplers.NearestNeighbor));
            outerLeftLeg.Mutate(x => x.Resize(40, 104, KnownResamplers.NearestNeighbor));
            Image<Rgba32> leftLeg = new(40, 104);
            leftLeg.Mutate(x => x.DrawImage(innerLeftLeg, new Point(4, 4), 1f));
            leftLeg.Mutate(x => x.DrawImage(outerLeftLeg, new Point(0, 0), 1f));
            innerLeftLeg.Dispose();
            outerLeftLeg.Dispose();

            leftLeg.Mutate(x => x.Resize(leftLeg.Width * ScaleSize, leftLeg.Height * ScaleSize, KnownResamplers.NearestNeighbor));

            return leftLeg;
        }
        public Image<Rgba32> CropDoubleSkinRightLeg(int ScaleSize = 8)
        {
            Image<Rgba32> innerRightLeg = ImageHelper.ConvertToImage(ImageBytes);
            Image<Rgba32> outerRightLeg = ImageHelper.ConvertToImage(ImageBytes);
            innerRightLeg.Mutate(x => x.Crop(new(4, 20, 4, 12)));
            outerRightLeg.Mutate(x => x.Crop(new(4, 36, 4, 12)));
            innerRightLeg.Mutate(x => x.Resize(32, 96, KnownResamplers.NearestNeighbor));
            outerRightLeg.Mutate(x => x.Resize(40, 104, KnownResamplers.NearestNeighbor));
            Image<Rgba32> leftLeg = new(40, 104);
            Image<Rgba32> rightLeg = new(40, 104);
            rightLeg.Mutate(x => x.DrawImage(innerRightLeg, new Point(4, 4), 1f));
            rightLeg.Mutate(x => x.DrawImage(outerRightLeg, new Point(0, 0), 1f));
            innerRightLeg.Dispose();
            outerRightLeg.Dispose();

            rightLeg.Mutate(x => x.Resize(rightLeg.Width * ScaleSize, rightLeg.Height * ScaleSize, KnownResamplers.NearestNeighbor));

            return rightLeg;
        }
    }
}
