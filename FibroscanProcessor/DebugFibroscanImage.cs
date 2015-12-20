using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using AForge;
using AForge.Imaging.Filters;
using Eklekto.Geometry;
using Eklekto.Imaging;
using Eklekto.Imaging.Binarization;
using Eklekto.Imaging.Filters;
using Eklekto.Imaging.Morfology;
using FibroscanProcessor.Elasto;
using FibroscanProcessor.Ultrasound;
using Point = System.Drawing.Point;

namespace FibroscanProcessor
{
    public partial class FibroscanImage
    {
        #region Debug Properties

        public Elastogram WorkingElasto
        {
            get
            {
                if (!_debugMode)
                    throw new AccessViolationException("Can`t use this method in production mode");

                return _workingElasto;
            }
        }

        public ElastoBlob WorkingBlob
        {
            get
            {
                if (!_debugMode)
                    throw new AccessViolationException("Can`t use this method in production mode");

                return _workingBlob;
            }
        }

        public UltrasoundModM WorkingUltrasoundModM
        {
            get
            {
                if (!_debugMode)
                    throw new AccessViolationException("Can`t use this method in production mode");

                return _workingUltrasoundModM;
            }
        }

        public UltrasoundModA WorkingUltrasoundModA
        {
            get
            {
                if (!_debugMode)
                    throw new AccessViolationException("Can`t use this method in production mode");

                return _workingUltrasoundModA;
            }
        }

        public Segment Fibroline
        {
            get
            {
                if (!_debugMode)
                    throw new AccessViolationException("Can`t use this method in production mode");

                return _fibroline;
            }
        }

        #endregion

        #region DebugElastogram

        public Image Step1LoadElastogram()
        {
            if (!_debugMode)
                throw new AccessViolationException("Can`t use this method in production mode");

            _workingElasto = LoadGrayElstogram();

            return _workingElasto.Image.Bitmap;
        }

        public Image Step2ElastoWithoutLine()
        {
            if (!_debugMode)
                throw new AccessViolationException("Can`t use this method in production mode");

            _workingElasto.GetFibroLine();
            _fibroline = _workingElasto.Fibroline;
            _workingElasto.PaintOverFibroline();
            
            return _workingElasto.Image.Bitmap;
        }

        public Image Step3KuwaharaElasto(int kernel)
        {
            if (!_debugMode)
                throw new AccessViolationException("Can`t use this method in production mode");

            Bitmap result = _workingElasto.Image.Bitmap.GrayscaleKuwahara(kernel);
            _workingElasto = new Elastogram(new SimpleGrayImage(result));

            return result;
        }

        public Image Step4SimpleBinarization(ref long timer, byte thresholdBin)
        {
            if (!_debugMode)
                throw new AccessViolationException("Can`t use this method in production mode");

            Stopwatch watch = Stopwatch.StartNew();

            _workingElasto.Image.ApplyBinarization(thresholdBin);

            watch.Stop();
            timer = watch.ElapsedMilliseconds;

            return _workingElasto.Image.Bitmap;
        }
        
        public Image Step4NiblackBinarization(ref long timer, double k=0.2, int radius = 20)
        {
            if (!_debugMode)
                throw new AccessViolationException("Can`t use this method in production mode");

            Stopwatch watch = Stopwatch.StartNew();

            Bitmap result = _workingElasto.Image.Bitmap.NiblackBinarization(k, radius);
            _workingElasto = new Elastogram(new SimpleGrayImage(result));

            watch.Stop();
            timer = watch.ElapsedMilliseconds;

            return result;
        }

        public Image Step4SauvolaBinarization(ref long timer, double k = 0.01, int radius = 20)
        {
            if (!_debugMode)
                throw new AccessViolationException("Can`t use this method in production mode");

            Stopwatch watch = Stopwatch.StartNew();

            Bitmap result = _workingElasto.Image.Bitmap.SauvolaBinarization(k, radius);
            _workingElasto = new Elastogram(new SimpleGrayImage(result));

            watch.Stop();
            timer = watch.ElapsedMilliseconds;

            return result;
        }

        public Image Step4WolfJoulionBinarization(ref long timer, double k = 0.2, int radius = 20)
        {
            if (!_debugMode)
                throw new AccessViolationException("Can`t use this method in production mode");

            Stopwatch watch = Stopwatch.StartNew();

            Bitmap result = _workingElasto.Image.Bitmap.WolfJoulionBinarization(k, radius);
            _workingElasto = new Elastogram(new SimpleGrayImage(result));

            watch.Stop();
            timer = watch.ElapsedMilliseconds;

            return _workingElasto.Image.Bitmap;
        }

        public Image Step4LgbtBinarization(ref long timer, double k, int localRadius, int globalRadius, byte globalThreshold)
        {
            if (!_debugMode)
                throw new AccessViolationException("Can`t use this method in production mode");

            Stopwatch watch = Stopwatch.StartNew();

            _workingElasto.Image.ApplyLgbtBinarization(k, localRadius, globalRadius, globalThreshold);

            watch.Stop();
            timer = watch.ElapsedMilliseconds;

            return _workingElasto.Image.Bitmap;
        }

        public Image Step4SimpleMorphologyBinarization(ref long timer, int morphologyKernel = 8, byte morphologyThreshold = 65)
        {
            if (!_debugMode)
                throw new AccessViolationException("Can`t use this method in production mode");

            Stopwatch watch = Stopwatch.StartNew();

            Bitmap result = _workingElasto.Image.Bitmap.MorphologySimpleBinarization(morphologyKernel, morphologyThreshold);
            _workingElasto = new Elastogram(new SimpleGrayImage(result));

            watch.Stop();
            timer = watch.ElapsedMilliseconds;

            return result;
        }

        public Image Step4MorphologyNiblackBinarization(ref long timer, double k = 0.2, int localRadius = 20,
            int globalRadius = 8, byte globalThreshold = 65)
        {
            if (!_debugMode)
                throw new AccessViolationException("Can`t use this method in production mode");

            Stopwatch watch = Stopwatch.StartNew();

            //WorkingElasto.Image.DrawVerticalGrayLine(0, 150, 85, 128);
            Bitmap result = _workingElasto.Image.Bitmap.MorphologyNiblackBinarization(k, localRadius, globalRadius, globalThreshold);
            _workingElasto = new Elastogram(new SimpleGrayImage(result));
            watch.Stop();
            timer = watch.ElapsedMilliseconds;

            return result;
        }

        public Image Step5EdgeRemoving(ref long timer, int leftDist1, int leftCentralDist1, int leftDist2, int leftCentralDist2, int rightDist, int rightCentralDist)
        {
            if (!_debugMode)
                throw new AccessViolationException("Can`t use this method in production mode");

            Stopwatch watch = Stopwatch.StartNew();

            _workingElasto.RemoveEdgeObjects(leftDist1, leftCentralDist1, leftDist2, leftCentralDist2, rightDist, rightCentralDist);

            watch.Stop();
            timer = watch.ElapsedMilliseconds;

            return _workingElasto.Image.Bitmap;
        }

        public Image Step6Morphology(int morphologyTimes)
        {
            if (!_debugMode)
                throw new AccessViolationException("Can`t use this method in production mode");

            Bitmap result = _workingElasto.Image.Bitmap.MorphologyOpening(morphologyTimes);
            _workingElasto = new Elastogram(new SimpleGrayImage(result));

            return _workingElasto.Image.Bitmap;
        }

        public Image Step7CropObjects(int step, int distance)
        {
            if (!_debugMode)
                throw new AccessViolationException("Can`t use this method in production mode");

            _workingElasto.CropObjects(step, distance);

            return _workingElasto.Image.Bitmap;
        }

        public Image Step8ChooseOneObject(ref long timer, double areaProportion, double heightProportion)
        {
            if (!_debugMode)
                throw new AccessViolationException("Can`t use this method in production mode");

            _workingElasto.ChooseContour(0.55, AreaMinLimit, 0.65);
            _workingBlob = _workingElasto.TargetObject;

            //_workingElasto.Image.DrawPolygon(_workingBlob.Contour.Points, 75, 4);

            if (_workingBlob == null)
                _elastoStatus = VerificationStatus.NotCalculated;

            return _workingElasto.Image.Bitmap;
        }

        public Image Step9Approximation(ref long timer, double sampleShare = SampleShare, double outliersShare = OutliersShare, int iterations = RansacIterations)
        {
            if (!_debugMode)
                throw new AccessViolationException("Can`t use this method in production mode");

            Stopwatch watch = Stopwatch.StartNew();

            _workingBlob.Approximate(ElastogramTopIndention, sampleShare, outliersShare, iterations);

            IntPoint p1 = new IntPoint(_workingBlob.LeftApproximation.GetX(ElastogramTopIndention), ElastogramTopIndention);
            IntPoint p2 = new IntPoint(_workingBlob.LeftApproximation.GetX(_workingElasto.Image.Cols - 1), _workingElasto.Image.Cols - 1);
            _workingElasto.Image.DrawGrayLine(p1, p2, 128);

            p1 = new IntPoint(_workingBlob.RightApproximation.GetX(ElastogramTopIndention), ElastogramTopIndention);
            p2 = new IntPoint(_workingBlob.RightApproximation.GetX(_workingElasto.Image.Cols - 1), _workingElasto.Image.Cols - 1);
            _workingElasto.Image.DrawGrayLine(p1, p2, 128);

            watch.Stop();
            timer = watch.ElapsedMilliseconds;

            return _workingElasto.Image.Bitmap;
        }

        public VerificationStatus Step10Classify()
        {
            if (!_debugMode)
                throw new AccessViolationException("Can`t use this method in production mode");

            ElastogramClassification rElasto = new ElastogramClassification();

            _elastoStatus = rElasto.Classiffy(_workingBlob, _fibroline);

            return _elastoStatus;
        }

        #endregion

        #region DebugUltrasoundM

        public Image Step11LoadUltrasoundM(double deviationThreshold, int deviationStreak)
        {
            if (!_debugMode)
                throw new AccessViolationException("Can`t use this method in production mode");

            _workingUltrasoundModM = LoadGrayUltrasoundModM(deviationThreshold, deviationStreak);

            return _workingUltrasoundModM.Image.Bitmap;
        }

        public Image Step12DrawBadLines(ref VerificationStatus result, int badLinesLimit)
        {
            if (!_debugMode)
                throw new AccessViolationException("Can`t use this method in production mode");
            List<int> badLines = _workingUltrasoundModM.DeviationStreakLines;
            SimpleGrayImage rez = new SimpleGrayImage(_workingUltrasoundModM.Image.Data);
            badLines.ForEach(line => rez.DrawHorisontalGrayLine(0, _workingUltrasoundModM.Image.Cols - 1, line, 0));

            _ultrasoundModeMStatus = badLines.Count < badLinesLimit ? VerificationStatus.Correct : VerificationStatus.Incorrect;
            result = _ultrasoundModeMStatus;

            return rez.Bitmap;
        }

        public Image Step13LoadUltrasoundA()
        {
            if (!_debugMode)
                throw new AccessViolationException("Can`t use this method in production mode");

            _workingUltrasoundModA = LoadGrayUltrasoundModA();

            return _workingUltrasoundModA.Image.Bitmap;
        }

        public Image Step14DrawUltraSoundApproximation(ref VerificationStatus result, int relativeEstimationLimit)
        {
            if (!_debugMode)
                throw new AccessViolationException("Can`t use this method in production mode");

            ReflectionedLine drawingLine = _workingUltrasoundModA.ApproxLine;
            SimpleGrayImage approxImage = new SimpleGrayImage(_workingUltrasoundModA.Image.Data);

            IntPoint startPoint = new IntPoint(drawingLine.GetX(ModATopIndention), ModATopIndention);
            IntPoint endPoint = new IntPoint(drawingLine.GetX(_workingUltrasoundModA.Image.Rows - ModABottomIndention),
                                                              _workingUltrasoundModA.Image.Rows - ModABottomIndention);
            approxImage.DrawGrayLine(startPoint, endPoint, 128);

            _ultrasoundModeAStatus = _workingUltrasoundModA.RelativeEstimation > relativeEstimationLimit ? VerificationStatus.Correct : VerificationStatus.Incorrect;
            result = _ultrasoundModeAStatus;

            return approxImage.Bitmap;
        }

        public Image Step15DrawBrightLines(ref VerificationStatus result, int threshold, int brightPixelLimit, int brightLinesLimit)
        {
            if (!_debugMode)
                throw new AccessViolationException("Can`t use this method in production mode");

            List<int> briteLines = _workingUltrasoundModM.getBrightLines(threshold, brightPixelLimit);
            SimpleGrayImage rez = new SimpleGrayImage(_workingUltrasoundModM.Image.Data);
            briteLines.ForEach(line => rez.DrawHorisontalGrayLine(0, _workingUltrasoundModM.Image.Cols - 1, line, 0));

            _ultrasoundModeMStatus = briteLines.Count < brightLinesLimit ? VerificationStatus.Correct : VerificationStatus.Incorrect;
            result = _ultrasoundModeMStatus;

            return rez.Bitmap;
        }
        #endregion
    }
}
