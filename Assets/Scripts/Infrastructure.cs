using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SourceDir
{
	North,
	East,
	South,
	West,
}

public class Line_FormCross
{
	public SourceDir dir;
	public float enterCoord;//y
	public float enterTime;//x
	public float timeLength;//平行四边形边长
	public float timeOffset;//平行四边形offset
}
public class Line_FormRoad
{
	public SourceDir dir;
	public float genCoord;
	public float genTime;
	public float genSpeed;
	public float genLength;
}

public class Line
{
	private Line_FormCross _lfc;
	private Line_FormRoad _lfr;
	private GameObject _lineObj;
	private Vector3 _velocity;
	private Vector3 _genPoint;

	public SourceDir Dir
	{
		get
		{
			return _lfc.dir;
		}
		set
		{
			_lfc.dir = value;
			_lfr.dir = value;
		}
	}
	public float Coord
	{
		get
		{
			return _lfc.enterCoord;
		}
		set
		{
			_lfc.enterCoord = value;
			_lfr.genCoord = value;
		}
	}
	public float EnterTime
	{
		get
		{
			return _lfc.enterTime;
		}
		set
		{
			_lfc.enterTime = value;
			Line_CrossToRoad(ref _lfc, ref _lfr);
		}
	}
	public float TimeLength
	{
		get
		{
			return _lfc.timeLength;
		}
		set
		{
			_lfc.timeLength = value;
			Line_CrossToRoad(ref _lfc, ref _lfr);
		}
	}
	public float TimeOffset
	{
		get
		{
			return _lfc.timeOffset;
		}
		set
		{
			_lfc.timeOffset = value;
			Line_CrossToRoad(ref _lfc, ref _lfr);
		}
	}
	public float GenTime
	{
		get
		{
			return _lfr.genTime;
		}
		set
		{
			_lfr.genTime = value;
			Line_RoadToCross(ref _lfr, ref _lfc);
		}
	}
	public float GenSpeed
	{
		get
		{
			return _lfr.genSpeed;
		}
		set
		{
			_lfr.genSpeed = value;
			Line_RoadToCross(ref _lfr, ref _lfc);
		}
	}
	public float GenLength
	{
		get
		{
			return _lfr.genLength;
		}
		set
		{
			_lfr.genLength = value;
			Line_RoadToCross(ref _lfr, ref _lfc);
		}
	}

	public Line(Line_FormCross lfc)
	{
		_lfc = lfc;
		_lfr = new Line_FormRoad();
		Line_CrossToRoad(ref _lfc, ref _lfr);
	}
	public Line(Line_FormRoad lfr)
	{
		_lfr = lfr;
		_lfc = new Line_FormCross();
		Line_RoadToCross(ref _lfr, ref _lfc);
	}

	private void Line_CrossToRoad(ref Line_FormCross lfcross, ref Line_FormRoad res)
	{
		res.dir = lfcross.dir;
		res.genCoord = lfcross.enterCoord;

		float crossLength = .0f;
		float fullLength = .0f;
		if (lfcross.dir == SourceDir.North || lfcross.dir == SourceDir.South)
		{
			crossLength = Configs.LENGTH_NS_CROSS;
			fullLength = Configs.LENGTH_NS_FULL;
		}
		else
		{
			crossLength = Configs.LENGTH_WE_CROSS;
			fullLength = Configs.LENGTH_WE_FULL;
		}
		float armLength = (fullLength - crossLength) / 2;
		res.genSpeed = crossLength / lfcross.timeOffset;
		res.genTime = lfcross.enterTime - armLength / res.genSpeed;
		res.genLength = lfcross.timeLength * res.genSpeed;

		return;
	}
	private void Line_RoadToCross(ref Line_FormRoad lfroad, ref Line_FormCross res)
	{
		res.dir = lfroad.dir;
		res.enterCoord = lfroad.genCoord;

		float crossLength = .0f;
		float fullLength = .0f;
		if (lfroad.dir == SourceDir.North || lfroad.dir == SourceDir.South)
		{
			crossLength = Configs.LENGTH_NS_CROSS;
			fullLength = Configs.LENGTH_NS_FULL;
		}
		else
		{
			crossLength = Configs.LENGTH_WE_CROSS;
			fullLength = Configs.LENGTH_WE_FULL;
		}
		float armLength = (fullLength - crossLength) / 2;

		res.enterTime = lfroad.genTime + armLength / lfroad.genSpeed;
		res.timeLength = lfroad.genLength / lfroad.genSpeed;
		res.timeOffset = crossLength / lfroad.genSpeed;

		return;
	}

	public void Simulate(float nowTime)
	{
		if(nowTime < GenTime)
		{
			return;
		}
		
		else
		{
			CheckObjInited();
			_lineObj.transform.position = _genPoint + _velocity * (nowTime - GenTime);
		}
	}

	public bool IsEnd(float nowTime)
	{
		return nowTime > GenTime + (GenLength + Utils.GetDirFullLength(Dir)) / GenSpeed;
	}
	public void EndSimulate()
	{
		if (_lineObj != null)
			GameObject.Destroy(_lineObj);
	}

	private void CheckObjInited()
	{
		if(_lineObj == null)
		{
			GameObject Prefab = (GameObject)Resources.Load("Cube");
			_lineObj = GameObject.Instantiate(Prefab);
			InitLineObjParas();
		}
	}
	private  void InitLineObjParas()
	{
		switch (Dir)
		{
			case SourceDir.North:
				_velocity = new Vector3(.0f, .0f, -GenSpeed);
				_lineObj.transform.localScale = new Vector3(Configs.GEN_LINE_THICKNESS, 1.0f, GenLength);
				_lineObj.transform.position =
					new Vector3(
						Coord - (Utils.GetDirCrossWidth(Dir) / 2.0f),
						.0f,
						(Utils.GetDirFullLength(Dir) / 2.0f) + (GenLength / 2.0f)
					);
				_genPoint = _lineObj.transform.position;
				break;
			case SourceDir.East:
				_velocity = new Vector3(-GenSpeed, .0f, .0f);
				_lineObj.transform.localScale = new Vector3(GenLength, 1.0f, Configs.GEN_LINE_THICKNESS);
				_lineObj.transform.position =
					new Vector3(
						(Utils.GetDirFullLength(Dir) / 2.0f) + (GenLength / 2.0f),
						.0f,
						-(Coord - (Utils.GetDirCrossWidth(Dir) / 2.0f))
					);
				_genPoint = _lineObj.transform.position;
				break;
			case SourceDir.South:
				_velocity = new Vector3(.0f, .0f, GenSpeed);
				_lineObj.transform.localScale = new Vector3(Configs.GEN_LINE_THICKNESS, 1.0f, GenLength);
				_lineObj.transform.position =
					new Vector3(
						-(Coord - (Utils.GetDirCrossWidth(Dir) / 2.0f)),
						.0f,
						-(Utils.GetDirFullLength(Dir) / 2.0f) - (GenLength / 2.0f)
					);
				_genPoint = _lineObj.transform.position;
				break;
			case SourceDir.West:
				_velocity = new Vector3(GenSpeed, .0f, .0f);
				_lineObj.transform.localScale = new Vector3(GenLength, 1.0f, Configs.GEN_LINE_THICKNESS);
				_lineObj.transform.position =
					new Vector3(
						-(Utils.GetDirFullLength(Dir) / 2.0f) - (GenLength / 2.0f),
						.0f,
						Coord - (Utils.GetDirCrossWidth(Dir) / 2.0f)
					);
				_genPoint = _lineObj.transform.position;
				break;
		}
		//float grayScale = Random.value;
		Material mat = _lineObj.GetComponent<MeshRenderer>().material;
		//mat.EnableKeyword("_EMISSION");
		mat.SetColor("_EmissionColor", new Color(Random.value, Random.value, Random.value) * (2.5f+0.2f*Random.value));
		return;
	}
}


public class Utils
{
	public static bool IsParallel(SourceDir myDir, SourceDir anotherDir)
	{
		int absOffset = Mathf.Abs(myDir - anotherDir);
		return absOffset == 0 || absOffset == 2;
	}

	public static bool IsLeftSide(SourceDir myDir, SourceDir anotherDir)
	{
		int offset = myDir - anotherDir;
		return offset == -1 || offset == 3;
	}

	public static bool IsRightSide(SourceDir myDir, SourceDir anotherDir)
	{
		int offset = myDir - anotherDir;
		return offset == 1 || offset == -3;
	}

	public static bool IsSameSide(SourceDir myDir, SourceDir anotherDir)
	{
		int offset = myDir - anotherDir;
		return offset == 0;
	}

	public static bool IsOppositeSide(SourceDir myDir, SourceDir anotherDir)
	{
		int absOffset = Mathf.Abs(myDir - anotherDir);
		return absOffset == 2;
	}

	public static bool IsNS(SourceDir dir)
	{
		return (dir == SourceDir.North) || (dir == SourceDir.South);
	}

	public static bool IsWE(SourceDir dir)
	{
		return (dir == SourceDir.West) || (dir == SourceDir.East);
	}

	public static float GetDirCrossLength(SourceDir dir)
	{
		if (IsNS(dir))
		{
			return Configs.LENGTH_NS_CROSS;
		}
		return Configs.LENGTH_WE_CROSS;
	}

	public static float GetDirFullLength(SourceDir dir)
	{
		if (IsNS(dir))
		{
			return Configs.LENGTH_NS_FULL;
		}
		return Configs.LENGTH_WE_FULL;
	}

	public static float GetDirCrossWidth(SourceDir dir)
	{
		if (IsNS(dir))
		{
			return Configs.LENGTH_WE_CROSS;
		}
		return Configs.LENGTH_NS_CROSS;
	}
}

public class Configs
{
	public const float LENGTH_NS_CROSS = 1.0f;
	public const float LENGTH_NS_FULL = 5.0f;
	public const float LENGTH_WE_CROSS = 1.0f;
	public const float LENGTH_WE_FULL = 5.0f;

	public const float GEN_INTERVAL = 0.02f;
	public const int GEN_MULTIPLIER = 1;

	public const float GEN_TIME_RANGE = 0.1f;
	public const float GEN_TIME_EXTEND = 0.01f;

	public const int GEN_ALL_TRY_TIMES = 5000;
	public const int GEN_RETRY_TIMES = 100;

	public const int GEN_STEP_SEARCH_TIMES = 10;

	public const float GEN_MIN_LENGTH = 0.6f;
	public const float GEN_MAX_LENGTH = 1.0f;
	public const float GEN_MIN_SPEED = 2f;
	public const float GEN_MAX_SPEED = 30f;
	public const float GEN_LINE_THICKNESS = 0.02f;
	public const int GEN_LINE_GAP_MULTIPLIER = 10;
}
	