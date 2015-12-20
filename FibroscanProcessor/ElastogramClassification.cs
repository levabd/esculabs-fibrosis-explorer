using System;
using Eklekto.Geometry;
using FibroscanProcessor.Elasto;

namespace FibroscanProcessor
{
    public class ElastogramClassification
    {
        const double RMax = 0.9;
        const double AMax = 0.92;
        const double StrongAngleDif = 5;
        const double WeakAngleDif = 12;//10;//8;
        const double AngleLimit = 12.5;

        public ElastoBlob TargetObject;
        public Segment FibroLine;

        public VerificationStatus Classiffy(ElastoBlob targetObject, Segment fibroLine)
        {
            TargetObject = targetObject;
            FibroLine = fibroLine;
            if (CheckForNull()) return VerificationStatus.NotCalculated;
            int area = TargetObject.Blob.Area;
            ReflectionedLine leftLine = TargetObject.LeftApproximation;
            ReflectionedLine rightLine = TargetObject.RightApproximation;
            double rSquareLeft = TargetObject.RSquareLeft;
            double rSquareRight = TargetObject.RSquareRight;
            double aLeft = TargetObject.RelativeEstimationLeft;
            double aRight = TargetObject.RelativeEstimationRight;

            if (area < 4000)
                return VerificationStatus.Uncertain;

            if (area < 6000)
            {
                if ((IsStrongAngleClose(leftLine, fibroLine.Equation)) &&
                    IsStrongAngleClose(leftLine, rightLine) &&
                    (IsGoodTilt(leftLine)) &&
                    IsGoodApproximation(leftLine, rSquareLeft, aLeft) &&
                    IsGoodApproximation(rightLine, rSquareRight, aRight))
                    return VerificationStatus.Correct;
                return VerificationStatus.Uncertain;
            }

            if (area < 12000)
            {
                if (!IsGoodApproximation(leftLine, rSquareLeft, aLeft) ||
                    !IsGoodApproximation(rightLine, rSquareRight, aRight))
                    return VerificationStatus.Uncertain;
                if ((IsWeakAngleClose(leftLine, fibroLine.Equation)) &&
                    IsWeakAngleClose(leftLine, rightLine) &&
                    (IsGoodTilt(leftLine)))
                    return VerificationStatus.Correct;
                return VerificationStatus.Incorrect;
            }

            if (area < 21000)
            {
                if (IsGoodApproximation(leftLine, rSquareLeft, aLeft) &&
                    IsGoodApproximation(rightLine, rSquareRight, aRight))
                    return VerificationStatus.Incorrect;
                return VerificationStatus.Uncertain;
            }

            return VerificationStatus.Uncertain;

            //Reserved code for classify
            if ((area < 8000) || (area > 6000))
            {

                if (IsGoodApproximation(leftLine, rSquareLeft, aLeft) &&
                   IsGoodApproximation(rightLine, rSquareRight, aRight))
                    return VerificationStatus.Correct;

                return VerificationStatus.Incorrect;
            }

            if ((area < 17500) || (area > 12000))
            {
                if (IsGoodApproximation(leftLine, rSquareLeft, aLeft) &&
                    IsGoodApproximation(rightLine, rSquareRight, aRight))
                    return VerificationStatus.Uncertain;
                return VerificationStatus.Incorrect;
            }
        }

        private bool IsStrongAngleClose(ReflectionedLine firstLine, ReflectionedLine secondLine)
        {
            if ((180 / Math.PI) * Math.Abs(Math.Atan(firstLine.A) - Math.Atan(secondLine.A)) < StrongAngleDif)
                return true;
            return false;
        }

        private bool IsWeakAngleClose(ReflectionedLine firstLine, ReflectionedLine secondLine)
        {
            if ((180 / Math.PI) * Math.Abs(Math.Atan(firstLine.A) - Math.Atan(secondLine.A)) < WeakAngleDif)
                return true;
            return false;
        }

        private bool IsGoodTilt(ReflectionedLine line)
        {
            if (Math.Atan(line.A) > 0)
                return true;
            else
                return false;
        }

        private bool IsGoodApproximation(ReflectionedLine approxLine, double rSquare, double relativeEstimation)
        {
            if ((180 / Math.PI) * Math.Abs(Math.Atan(approxLine.A)) < AngleLimit)
                //if (relativeEstimation > aMax)
                    return true;
                //else
                  //  return false;
            else if (rSquare > RMax)
                    return true;
            return false;
        }

        private bool CheckForNull()
        {
            if (TargetObject.Blob.Area == 0)
                return true;

            if (TargetObject.LeftApproximation == null)
                return true;

            if (TargetObject.RightApproximation == null)
                return true;

            if (FibroLine == null)
                return true;

            if (Math.Abs(TargetObject.RSquareLeft) < Double.Epsilon)
                return true;

            if (Math.Abs(TargetObject.RSquareLeft) < Double.Epsilon)
                return true;

            if (Math.Abs(TargetObject.RSquareRight) < Double.Epsilon)
                return true;

            if (Math.Abs(TargetObject.RelativeEstimationRight) < Double.Epsilon)
                return true;

            if (Math.Abs(TargetObject.RelativeEstimationLeft) < Double.Epsilon)
                return true;
            return false;
        }
    }
}
