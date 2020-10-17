using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public interface ILineGenerator
{
	 Line GenerateLineOnce(List<Line> allLines,float nowTime);
}
public class LineGenerator_MonteCarlo:ILineGenerator
{
	public Line GenerateLineOnce(List<Line> allLines,float nowTime)
	{
		bool isOK = true;
		Line myLine = null;

		for (int genCount = 0; genCount < Configs.GEN_ALL_TRY_TIMES ;genCount++)
		{
			myLine = GetRandomLine(nowTime, genCount);
			isOK = true;
			foreach(Line otherLine in allLines)
			{
				if (!IsCompatible(myLine, otherLine))
				{
					isOK = false;
					break;
				}
			}
			if(isOK == true)
			{
				break;
			}
		}

		if (isOK)
		{
			return myLine;
		}
		else
		{
			return null;
		}
	}

	public Line GenerateLineOnce(List<Line> allLines, float nowTime,SourceDir dir)
	{
		bool isOK = true;
		Line myLine = null;
		int stepSearchCount = 0;
		for (int genCount = 0; genCount < 1000; genCount++)
		{
			if(stepSearchCount % 10 == 0)
			{
				myLine = GetRandomLine(nowTime, genCount, dir);
			}
			else
			{
				AdjustLine(ref myLine);
			}
			stepSearchCount++;

			isOK = true;
			foreach (Line otherLine in allLines)
			{
				if (!IsCompatible(myLine, otherLine))
				{
					isOK = false;
					break;
				}
			}
			if (isOK == true)
			{
				break;
			}
		}

		if (isOK)
		{
			return myLine;
		}
		else
		{
			return null;
		}
	}


	private Line GetRandomLine(float nowTime,int genCount)
	{
		SourceDir genDir = (SourceDir)(Random.Range(.0f, .999f) * 4);

		float timeRange = Configs.GEN_TIME_RANGE + Configs.GEN_TIME_EXTEND * (genCount/Configs.GEN_RETRY_TIMES);
		float genTime = nowTime + Random.value * timeRange;

		float genCoord = Random.value * Utils.GetDirCrossWidth(genDir);

		float genLength = Random.value * (Configs.GEN_MAX_LENGTH - Configs.GEN_MIN_LENGTH) + Configs.GEN_MIN_LENGTH;

		float genSpeed = Random.value * (Configs.GEN_MAX_SPEED - Configs.GEN_MIN_SPEED) + Configs.GEN_MIN_SPEED;

		Line_FormRoad lfr = new Line_FormRoad();
		lfr.dir = genDir;
		lfr.genTime = genTime;
		lfr.genCoord = genCoord;
		lfr.genLength = genLength;
		lfr.genSpeed = genSpeed;

		Line res = new Line(lfr);
		return res;
	}

	private Line GetRandomLine(float nowTime, int genCount,SourceDir dir)
	{
		SourceDir genDir = dir;

		float timeRange = Configs.GEN_TIME_RANGE + Configs.GEN_TIME_EXTEND * (genCount / Configs.GEN_RETRY_TIMES);
		float genTime = nowTime + Random.value * timeRange;

		float genCoord = Random.value * Utils.GetDirCrossWidth(genDir);

		float genLength = Random.value * (Configs.GEN_MAX_LENGTH - Configs.GEN_MIN_LENGTH) + Configs.GEN_MIN_LENGTH;

		float genSpeed = Random.value * (Configs.GEN_MAX_SPEED - Configs.GEN_MIN_SPEED) + Configs.GEN_MIN_SPEED;

		Line_FormRoad lfr = new Line_FormRoad();
		lfr.dir = genDir;
		lfr.genTime = genTime;
		lfr.genCoord = genCoord;
		lfr.genLength = genLength;
		lfr.genSpeed = genSpeed;

		Line res = new Line(lfr);
		return res;
	}

	private void AdjustLine(ref Line myLine)
	{
		myLine.GenSpeed = Random.value * (Configs.GEN_MAX_SPEED - Configs.GEN_MIN_SPEED) + Configs.GEN_MIN_SPEED;
	}

	private bool IsCompatible(Line myLine, Line otherLine)
	{
		if (Utils.IsLeftSide(myLine.Dir, otherLine.Dir))
		{
			return IsCompatible_Left(myLine, otherLine);
		}
		else if (Utils.IsRightSide(myLine.Dir, otherLine.Dir))
		{
			return IsCompatible_Right(myLine, otherLine);
		}
		else if (Utils.IsSameSide(myLine.Dir, otherLine.Dir))
		{
			return IsCompatible_SameSide(myLine, otherLine);
		}
		else if (Utils.IsOppositeSide(myLine.Dir, otherLine.Dir))
		{
			return IsCompatible_OppositeSide(myLine, otherLine);
		}
		return false;
	}

	public bool IsCompatible_Right(Line myLine, Line otherLine)
	{
		bool lowerPos = myLine.EnterTime < ((myLine.Coord * otherLine.TimeOffset) / Utils.GetDirCrossWidth(myLine.Dir)) + otherLine.EnterTime;
		if (lowerPos)
		{
			if (myLine.TimeLength > 0 &&
				myLine.TimeLength < ((myLine.Coord * otherLine.TimeOffset) / Utils.GetDirCrossWidth(myLine.Dir)) + otherLine.EnterTime - myLine.EnterTime)
			{
				if (myLine.TimeOffset > 0 &&
					myLine.TimeOffset < (((myLine.Coord * otherLine.TimeOffset) / Utils.GetDirCrossWidth(myLine.Dir)) + otherLine.EnterTime - myLine.EnterTime - myLine.TimeLength) * (Utils.GetDirCrossLength(myLine.Dir) /(Utils.GetDirCrossWidth(otherLine.Dir) - otherLine.Coord)))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}
		else
		{
			if (myLine.TimeOffset > (((myLine.Coord * otherLine.TimeOffset) / Utils.GetDirCrossWidth(myLine.Dir)) + otherLine.EnterTime - myLine.EnterTime + otherLine.TimeLength) * (Utils.GetDirCrossLength(myLine.Dir) / (Utils.GetDirCrossWidth(otherLine.Dir) - otherLine.Coord)))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		return false;
	}

	public bool IsCompatible_Left(Line myLine, Line otherLine)
	{
		bool lowerPos = myLine.EnterTime < (((Utils.GetDirCrossWidth(myLine.Dir) - myLine.Coord) * otherLine.TimeOffset)/ Utils.GetDirCrossWidth(myLine.Dir)) + otherLine.EnterTime;
		if (lowerPos)
		{
			if (myLine.TimeLength > 0 &&
				myLine.TimeLength < (((Utils.GetDirCrossWidth(myLine.Dir) - myLine.Coord) * otherLine.TimeOffset) / Utils.GetDirCrossWidth(myLine.Dir)) + otherLine.EnterTime - myLine.EnterTime)
			{ 
				if(myLine.TimeOffset > 0 &&
					myLine.TimeOffset < ((((Utils.GetDirCrossWidth(myLine.Dir) - myLine.Coord) * otherLine.TimeOffset) / Utils.GetDirCrossWidth(myLine.Dir)) + otherLine.EnterTime - myLine.EnterTime - myLine.TimeLength) * (Utils.GetDirCrossLength(myLine.Dir) / otherLine.Coord))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}
		else
		{
			if (myLine.TimeOffset > ((((Utils.GetDirCrossWidth(myLine.Dir) - myLine.Coord) * otherLine.TimeOffset) / Utils.GetDirCrossWidth(myLine.Dir)) + otherLine.EnterTime - myLine.EnterTime + otherLine.TimeLength) * (Utils.GetDirCrossLength(myLine.Dir) / otherLine.Coord))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		return false;
	}

	public bool IsCompatible_SameSide(Line myLine, Line otherLine)
	{
		if(Mathf.Abs(myLine.Coord - otherLine.Coord) > Configs.GEN_LINE_GAP_MULTIPLIER * Configs.GEN_LINE_THICKNESS)
		{
			return true;
		}
		//不等就可以随意！
		if (myLine.GenTime < otherLine.GenTime)
		{
			if (((otherLine.GenTime-myLine.GenTime)*myLine.GenSpeed - myLine.GenLength)>0 &&
				(((myLine.GenTime - otherLine.GenTime + ((Utils.GetDirFullLength(myLine.Dir)+myLine.GenLength)/myLine.GenSpeed))*(otherLine.GenSpeed - myLine.GenSpeed))< ((otherLine.GenTime - myLine.GenTime) * myLine.GenSpeed - myLine.GenLength)))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		else
		{
			if (((myLine.GenTime - otherLine.GenTime)*otherLine.GenSpeed - otherLine.GenLength)>0 &&
				(((otherLine.GenTime - myLine.GenTime + ((Utils.GetDirFullLength(myLine.Dir)+otherLine.GenLength)/otherLine.GenSpeed))*(myLine.GenSpeed - otherLine.GenSpeed)) < ((myLine.GenTime - otherLine.GenTime) * otherLine.GenSpeed - otherLine.GenLength)))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		return false;
	}

	public bool IsCompatible_OppositeSide(Line myLine, Line otherLine)
	{
		if (Mathf.Abs(myLine.Coord - (Utils.GetDirCrossWidth(myLine.Dir) - otherLine.Coord)) > Configs.GEN_LINE_GAP_MULTIPLIER * Configs.GEN_LINE_THICKNESS)
		{
			return true;
		}
		//不等就可以随意！
		if ((myLine.GenTime + (Utils.GetDirFullLength(myLine.Dir)+myLine.GenLength)/myLine.GenSpeed < otherLine.GenTime) ||
			(myLine.GenTime > otherLine.GenTime + ((Utils.GetDirFullLength(myLine.Dir) + otherLine.GenLength) / (otherLine.GenSpeed))))
		{
			return true;
		}
		else
		{
			return false;
		}
		return false;
	}

}

