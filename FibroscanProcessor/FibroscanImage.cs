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
    public enum VerificationStatus
    {
        NotCalculated,
        Incorrect,
        Uncertain,
        Correct
    }

    public partial class FibroscanImage
    {
        #region Const

        // ReSharper disable InconsistentNaming
        // Const
        private Rectangle ElastogramRect = new Rectangle(242, 33, 253, 194);
        private Rectangle UltrasoundModMRect = new Rectangle(50, 33, 92, 254);
        private Rectangle UltrasoundModARect = new Rectangle(160, 33, 60, 254);

        private const int KuwaharaKernel = 29;
        private const byte GlobalThreshold = 65;
        private const double MorphologyBin_K = 0.2;
        private const byte MorphologyBinThreshold = 65;
        private const int MorphologyBinLocalRadius = 20;
        private const int MorphologyBinGlobalRadius = 8;
        private const int LeftEdgeDistance1 = 25;
        private const int LeftCentralEdgeDist1 = 85;
        private const int LeftEdgeDistance2 = 90;
        private const int LeftCentralEdgeDist2 = 100;
        private const int RightEdgeDistance = 50;
        private const int RightCentralEdgeDist = 80;
        private const int MorphologyOpeningKernel = 4;
        private const int CropSteps = 20;
        private const int CropDistance = 8;
        private const int AreaMinLimit = 4000;
        private const double AreaProportion = 0.6;
        private const double HeightProportion = 0.65;
        private const double SampleShare = 0.15;
        private const double OutliersShare = 0.2;
        private const int RansacIterations = 12000;
        private const int ElastogramTopIndention = 20;

        private const int UsDeviationThreshold = 30;
        private const int UsDeviationStreak = 3;
        private const int BadLinesLimit = 50;
        private const int ModMTopIndention = 20;
        private const int ModMBottomIndention = 60;
        //alternative mod m
        private const int ModMBrightLinesLimit = 30;
        private const int ModMThreshold = 246;
        private const int ModMBrightPixelLimit = 30;

        private const int ModARelativeEstimationLimit = 85;
        private const int ModATopIndention = 20;
        private const int ModABottomIndention = 20;
        // ReSharper restore InconsistentNaming

        #endregion

        #region Private variables

        private Elastogram _workingElasto;
        private ElastoBlob _workingBlob;
        private UltrasoundModM _workingUltrasoundModM;
        private UltrasoundModA _workingUltrasoundModA;
        private Segment _fibroline;

        private readonly Image _source;

        private VerificationStatus _elastoStatus = VerificationStatus.NotCalculated;
        private VerificationStatus _ultrasoundModeMStatus = VerificationStatus.NotCalculated;
        private VerificationStatus _ultrasoundModeAStatus = VerificationStatus.NotCalculated;
        private double _stiffness = 0; //

        private readonly bool _debugMode;

        #endregion

        #region Constructor

        public FibroscanImage(Image source, bool debugMode = false)
        {
            _debugMode = debugMode;
            _source = source;
        }

        #endregion

        #region Properties

        public Image Merged
        {
            get
            {
                if ((_workingElasto == null) ||
                    (_workingBlob == null) ||
                    (_workingUltrasoundModM == null) ||
                    (_workingUltrasoundModA == null))
                {
                    Proceed();
                }

                // Already checked by null
                // ReSharper disable PossibleNullReferenceException
                Point p1 = new Point(_workingBlob.LeftApproximation.GetX(0) + ElastogramRect.X, 0 + ElastogramRect.Y);
                Point p2 = new Point(_workingBlob.LeftApproximation.GetX(_workingElasto.Image.Cols - 1) + ElastogramRect.X, _workingElasto.Image.Cols - 1 + ElastogramRect.Y);
                Point p3 = new Point(_workingBlob.RightApproximation.GetX(0) + ElastogramRect.X, 0 + ElastogramRect.Y);
                Point p4 = new Point(_workingBlob.RightApproximation.GetX(_workingElasto.Image.Cols - 1) + ElastogramRect.X, _workingElasto.Image.Cols - 1 + ElastogramRect.Y);

                Point[] pContour = new Point[_workingBlob.Contour.Points.Count];
                for (int i = 0; i < _workingBlob.Contour.Points.Count; i++)
                    pContour[i] = new Point(_workingBlob.Contour.Points[i].X + ElastogramRect.X, _workingBlob.Contour.Points[i].Y + ElastogramRect.Y);

                using (Graphics g = Graphics.FromImage(_source))
                {
                    g.DrawPolygon(new Pen(Color.GreenYellow), pContour);
                    g.DrawLine(new Pen(Color.Red), p1, p2);
                    g.DrawLine(new Pen(Color.Blue), p3, p4);
                    /*_workingUltrasoundModM.DeviationStreakLines.ForEach(point => g.DrawLine(new Pen(Color.Blue), 
                        new Point(0 + UltrasoundModMRect.X, point + UltrasoundModMRect.Y), 
                        new Point(UltrasoundModMRect.Width + UltrasoundModMRect.X, point + UltrasoundModMRect.Y)));
                    g.DrawLine(new Pen(Color.Blue), new Point(_workingUltrasoundModA.ApproxLine.GetX(0) + UltrasoundModARect.X, 0 + UltrasoundModARect.Y),
                        new Point(_workingUltrasoundModA.ApproxLine.GetX(UltrasoundModARect.Height) + UltrasoundModARect.X, UltrasoundModARect.Height + UltrasoundModARect.Y));
                    */
                    _workingUltrasoundModM.getBrightLines(ModMThreshold, ModMBrightPixelLimit).ForEach(point => g.DrawLine(new Pen(Color.Blue), 
                        new Point(0 + UltrasoundModMRect.X, point + UltrasoundModMRect.Y), 
                        new Point(UltrasoundModMRect.Width + UltrasoundModMRect.X, point + UltrasoundModMRect.Y)));
                    g.DrawLine(new Pen(Color.Blue), new Point(_workingUltrasoundModA.ApproxLine.GetX(0) + UltrasoundModARect.X, 0 + UltrasoundModARect.Y),
                        new Point(_workingUltrasoundModA.ApproxLine.GetX(UltrasoundModARect.Height) + UltrasoundModARect.X, UltrasoundModARect.Height + UltrasoundModARect.Y));
                    
                }
                // ReSharper restore PossibleNullReferenceException

                return _source;
            }
        }

        public VerificationStatus UltrasoundModeMStatus
        {
            get
            {
                if (_ultrasoundModeMStatus == VerificationStatus.NotCalculated)
                    Proceed();
                return _ultrasoundModeMStatus;
            }
        }

        public VerificationStatus UltrasoundModeAStatus
        {
            get
            {
                if (_ultrasoundModeAStatus == VerificationStatus.NotCalculated)
                    Proceed();
                return _ultrasoundModeAStatus;
            }
        }

        public VerificationStatus ElastoStatus
        {
            get
            {
                if (_elastoStatus == VerificationStatus.NotCalculated)
                    Proceed();
                return _elastoStatus;
            }
        }

        public double Stiffness
        {
            get
            {
                if (_stiffness < Double.Epsilon)
                    Proceed();
                return _stiffness;
            }
        }

        #endregion

        #region Entry Point

        private void Proceed()
        {
            _workingElasto = LoadGrayElstogram();
            _elastoStatus = VerifyElasto(ref _workingElasto, out _workingBlob);

            _workingUltrasoundModM = LoadGrayUltrasoundModM(UsDeviationThreshold, UsDeviationStreak);
            //_ultrasoundModeMStatus = _workingUltrasoundModM.DeviationStreakLines.Count < BadLinesLimit ? VerificationStatus.Correct : VerificationStatus.Incorrect;
            _ultrasoundModeMStatus = _workingUltrasoundModM.getBrightLines(ModMThreshold,ModMBrightPixelLimit).Count < ModMBrightLinesLimit ? VerificationStatus.Correct : VerificationStatus.Incorrect;

            _workingUltrasoundModA = LoadGrayUltrasoundModA();
            _ultrasoundModeAStatus = _workingUltrasoundModA.RelativeEstimation > ModARelativeEstimationLimit ? VerificationStatus.Correct : VerificationStatus.Incorrect;
        }

        private VerificationStatus VerifyElasto(ref Elastogram workingElasto, out ElastoBlob workingBlob)
        {

            workingElasto.GetFibroLine();
            _fibroline = workingElasto.Fibroline;
            workingElasto.PaintOverFibroline();
            workingElasto = new Elastogram(new SimpleGrayImage(workingElasto.Image.Bitmap.GrayscaleKuwahara(KuwaharaKernel)));

            _workingElasto = new Elastogram(new SimpleGrayImage(_workingElasto.Image.Bitmap.MorphologyNiblackBinarization(
                MorphologyBin_K, MorphologyBinLocalRadius, MorphologyBinGlobalRadius, MorphologyBinThreshold)));

            workingElasto.RemoveEdgeObjects(LeftEdgeDistance1, LeftCentralEdgeDist1,
                                            LeftEdgeDistance2, LeftCentralEdgeDist2, 
                                            RightEdgeDistance, RightCentralEdgeDist);

            workingElasto = new Elastogram(new SimpleGrayImage(workingElasto.Image.Bitmap.MorphologyOpening(MorphologyOpeningKernel)));

            workingElasto.CropObjects(CropSteps, CropDistance);

            workingElasto.ChooseContour(AreaProportion, AreaMinLimit, HeightProportion);

            workingBlob = workingElasto.TargetObject;

            if (workingBlob == null)
                return VerificationStatus.NotCalculated;

            workingBlob.Approximate(ElastogramTopIndention, SampleShare, OutliersShare, RansacIterations);
                return (new ElastogramClassification()).Classiffy(workingBlob, _fibroline);
        }

        #endregion

        #region Private

        private Elastogram LoadGrayElstogram()
        {
            Bitmap target = new Bitmap(ElastogramRect.Width, ElastogramRect.Height);
            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(_source, new Rectangle(0, 0, target.Width, target.Height),
                                 ElastogramRect, GraphicsUnit.Pixel);
            }

            return new Elastogram(new SimpleGrayImage(Grayscale.CommonAlgorithms.RMY.Apply(target)));
        }

        private UltrasoundModM LoadGrayUltrasoundModM(double deviationThreshold, int deviationStreakSize)
        {
            Bitmap target = new Bitmap(UltrasoundModMRect.Width, UltrasoundModMRect.Height);
            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(_source, new Rectangle(0, 0, target.Width, target.Height),
                                 UltrasoundModMRect, GraphicsUnit.Pixel);
            }

            return new UltrasoundModM(deviationThreshold, deviationStreakSize, new SimpleGrayImage(Grayscale.CommonAlgorithms.RMY.Apply(target)),
                                      ModMTopIndention, ModMBottomIndention);
        }

        private UltrasoundModA LoadGrayUltrasoundModA()
        {
            Bitmap target = new Bitmap(UltrasoundModARect.Width, UltrasoundModARect.Height);
            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(_source, new Rectangle(0, 0, target.Width, target.Height),
                                 UltrasoundModARect, GraphicsUnit.Pixel);
            }

            return new UltrasoundModA(new SimpleGrayImage(Grayscale.CommonAlgorithms.RMY.Apply(target)), ModATopIndention, ModABottomIndention);
        }

        #endregion
    }
}
