// MIT License
// Copyright (c) 2016 Geometry Gym Pty Ltd

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
// and associated documentation files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial 
// portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
// LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.ComponentModel;
using System.Linq;
using System.Drawing;
using GeometryGym.STEP;

namespace GeometryGym.Ifc
{
	public partial interface IfcUnit : IfcInterface { } // = SELECT(IfcDerivedUnit, IfcNamedUnit, IfcMonetaryUnit);
	public class IfcUnitaryControlElement : IfcDistributionControlElement //IFC4  
	{
		internal IfcUnitaryControlElementTypeEnum mPredefinedType = IfcUnitaryControlElementTypeEnum.NOTDEFINED;
		public IfcUnitaryControlElementTypeEnum PredefinedType { get { return mPredefinedType; } set { mPredefinedType = value; } }
		internal IfcUnitaryControlElement(IfcUnitaryControlElement a) : base(a) { mPredefinedType = a.mPredefinedType; }
		internal IfcUnitaryControlElement() : base() { }
		internal IfcUnitaryControlElement(IfcProduct host, IfcObjectPlacement p, IfcProductRepresentation r, IfcDistributionSystem system) : base(host,p,r, system) { }
		internal static void parseFields(IfcUnitaryControlElement a, List<string> arrFields, ref int ipos)
		{
			IfcDistributionControlElement.parseFields(a, arrFields, ref ipos);
			string s = arrFields[ipos++];
			if (s.StartsWith("."))
				a.mPredefinedType = (IfcUnitaryControlElementTypeEnum)Enum.Parse(typeof(IfcUnitaryControlElementTypeEnum), s.Replace(".", ""));
		}
		internal new static IfcUnitaryControlElement Parse(string strDef) { IfcUnitaryControlElement d = new IfcUnitaryControlElement(); int ipos = 0; parseFields(d, ParserSTEP.SplitLineFields(strDef), ref ipos); return d; }
		protected override string BuildString()
		{
			return base.BuildString() + (mDatabase.mSchema == Schema.IFC2x3 ? "" : (mPredefinedType == IfcUnitaryControlElementTypeEnum.NOTDEFINED ? ",$" : ",." + mPredefinedType.ToString() + "."));
		}
	}
	public class IfcUnitaryControlElementType : IfcDistributionControlElementType
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCUNITARYCONTROLELEMENTTYPE";
		internal IfcUnitaryControlElementTypeEnum mPredefinedType = IfcUnitaryControlElementTypeEnum.NOTDEFINED;// : IfcUnitaryControlElementTypeEnum; 
		public IfcUnitaryControlElementTypeEnum PredefinedType { get { return mPredefinedType; } set { mPredefinedType = value; } }
		internal IfcUnitaryControlElementType(IfcUnitaryControlElementType be) : base(be) { mPredefinedType = be.mPredefinedType; }
		internal IfcUnitaryControlElementType() : base() { }
		internal IfcUnitaryControlElementType(DatabaseIfc m, string name, IfcUnitaryControlElementTypeEnum t) : base(m) { Name = name; mPredefinedType = t; }
		internal static void parseFields(IfcUnitaryControlElementType t, List<string> arrFields, ref int ipos) { IfcDistributionControlElementType.parseFields(t, arrFields, ref ipos); t.mPredefinedType = (IfcUnitaryControlElementTypeEnum)Enum.Parse(typeof(IfcUnitaryControlElementTypeEnum), arrFields[ipos++].Replace(".", "")); }
		internal new static IfcUnitaryControlElementType Parse(string strDef) { IfcUnitaryControlElementType t = new IfcUnitaryControlElementType(); int ipos = 0; parseFields(t, ParserSTEP.SplitLineFields(strDef), ref ipos); return t; }
		protected override string BuildString() { return base.BuildString() + ",." + mPredefinedType.ToString() + "."; }
	}
	public class IfcUnitaryEquipment : IfcEnergyConversionDevice //IFC4
	{
		internal IfcUnitaryEquipmentTypeEnum mPredefinedType = IfcUnitaryEquipmentTypeEnum.NOTDEFINED;// OPTIONAL : IfcUnitaryEquipmentTypeEnum;
		public IfcUnitaryEquipmentTypeEnum PredefinedType { get { return mPredefinedType; } set { mPredefinedType = value; } }
		internal IfcUnitaryEquipment() : base() { }
		internal IfcUnitaryEquipment(IfcUnitaryEquipment b) : base(b) { mPredefinedType = b.mPredefinedType; }
		internal IfcUnitaryEquipment(IfcProduct host, IfcObjectPlacement placement, IfcProductRepresentation representation, IfcDistributionSystem system) : base(host, placement, representation, system) { }

		internal static void parseFields(IfcUnitaryEquipment s, List<string> arrFields, ref int ipos)
		{
			IfcEnergyConversionDevice.parseFields(s, arrFields, ref ipos);
			string str = arrFields[ipos++];
			if (str[0] == '.')
				s.mPredefinedType = (IfcUnitaryEquipmentTypeEnum)Enum.Parse(typeof(IfcUnitaryEquipmentTypeEnum), str);
		}
		internal new static IfcUnitaryEquipment Parse(string strDef) { IfcUnitaryEquipment s = new IfcUnitaryEquipment(); int ipos = 0; parseFields(s, ParserSTEP.SplitLineFields(strDef), ref ipos); return s; }
		protected override string BuildString()
		{
			return base.BuildString() + (mDatabase.mSchema == Schema.IFC2x3 ? "" : (mPredefinedType == IfcUnitaryEquipmentTypeEnum.NOTDEFINED ? ",$" : ",." + mPredefinedType.ToString() + "."));
		}
	}
	public class IfcUnitaryEquipmentType : IfcEnergyConversionDeviceType
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCUNITARYEQUIPMENTTYPE";
		internal IfcUnitaryEquipmentTypeEnum mPredefinedType = IfcUnitaryEquipmentTypeEnum.NOTDEFINED;// : IfcUnitaryEquipmentTypeEnum; 
		public IfcUnitaryEquipmentTypeEnum PredefinedType { get { return mPredefinedType; } set { mPredefinedType = value; } }
		internal IfcUnitaryEquipmentType() : base() { }
		internal IfcUnitaryEquipmentType(IfcUnitaryEquipmentType be) : base((IfcEnergyConversionDeviceType)be) { mPredefinedType = be.mPredefinedType; }
		internal static void parseFields(IfcUnitaryEquipmentType t, List<string> arrFields, ref int ipos) { IfcEnergyConversionDeviceType.parseFields(t, arrFields, ref ipos); t.mPredefinedType = (IfcUnitaryEquipmentTypeEnum)Enum.Parse(typeof(IfcUnitaryEquipmentTypeEnum), arrFields[ipos++].Replace(".", "")); }
		internal new static IfcUnitaryEquipmentType Parse(string strDef) { IfcUnitaryEquipmentType t = new IfcUnitaryEquipmentType(); int ipos = 0; parseFields(t, ParserSTEP.SplitLineFields(strDef), ref ipos); return t; }
		protected override string BuildString() { return base.BuildString() + ",." + mPredefinedType.ToString() + "."; }
	}
	public partial class IfcUnitAssignment : BaseClassIfc
	{
		public enum Length { Metre, Centimetre, Millimetre, Foot, Inch };

		private List<int> mUnits = new List<int>();// : SET [1:?] OF IfcUnit; 

		internal List<IfcUnit> Units { get { return mUnits.ConvertAll(x => mDatabase.mIfcObjects[x] as IfcUnit); } }

		internal IfcUnitAssignment() : base() { }
		internal IfcUnitAssignment(IfcUnitAssignment v) : base() { mUnits = new List<int>(v.mUnits.ToArray()); }
		internal IfcUnitAssignment(DatabaseIfc m) : base(m) { }

		internal void SetUnits(Length length)
		{
			if (length == Length.Millimetre)
			{
				mUnits.Add(new IfcSIUnit(mDatabase, IfcUnitEnum.LENGTHUNIT, IfcSIPrefix.MILLI, IfcSIUnitName.METRE).mIndex);
				mDatabase.ScaleSI = 0.001;
			}
			else if (length == Length.Centimetre)
			{
				mUnits.Add(new IfcSIUnit(mDatabase, IfcUnitEnum.LENGTHUNIT, IfcSIPrefix.CENTI, IfcSIUnitName.METRE).mIndex);
				mDatabase.ScaleSI = 0.01;

			}
			else if (length == Length.Inch)
			{
				IfcMeasureWithUnit mwu = new IfcMeasureWithUnit(new IfcLengthMeasure(0.0254), mDatabase.mSILength);
				mUnits.Add(new IfcConversionBasedUnit(IfcUnitEnum.LENGTHUNIT, "Inches", mwu).mIndex);
				mDatabase.ScaleSI = 0.0254;
			}
			else if (length == Length.Foot)
			{
				IfcMeasureWithUnit mwu = new IfcMeasureWithUnit(new IfcLengthMeasure(1 / 3.2808399), mDatabase.mSILength);
				mUnits.Add(new IfcConversionBasedUnit(IfcUnitEnum.LENGTHUNIT, "Feet",mwu).mIndex);
				mDatabase.ScaleSI = 1 / 3.2808399;
			}
			else
				mUnits.Add(mDatabase.mSILength.mIndex);
			SetUnits();
		}
		internal void SetUnits()
		{
			if (Find(IfcUnitEnum.AREAUNIT) == null && mDatabase.mSIArea != null)
				mUnits.Add(mDatabase.mSIArea.mIndex);
			if (Find(IfcUnitEnum.VOLUMEUNIT) == null && mDatabase.mSIVolume != null)
				mUnits.Add(mDatabase.mSIVolume.mIndex);
			if (Find(IfcUnitEnum.PLANEANGLEUNIT) == null)
			{
				IfcSIUnit radians = new IfcSIUnit(mDatabase, IfcUnitEnum.PLANEANGLEUNIT, IfcSIPrefix.NONE, IfcSIUnitName.RADIAN);
				mUnits.Add(mDatabase.mPlaneAngleToRadians == 1 ? radians.mIndex : new IfcConversionBasedUnit(IfcUnitEnum.PLANEANGLEUNIT, "DEGREE", new IfcMeasureWithUnit(new IfcPlaneAngleMeasure(Math.PI / 180.0), radians)).mIndex);
			}
			if (Find(IfcUnitEnum.TIMEUNIT) == null)
			{
				IfcSIUnit seconds = new IfcSIUnit(mDatabase, IfcUnitEnum.TIMEUNIT, IfcSIPrefix.NONE, IfcSIUnitName.SECOND);
				if (mDatabase.mTimeInDays)
				{
					IfcMeasureWithUnit mu = new IfcMeasureWithUnit(new IfcTimeMeasure(60 * 60 * 24), seconds);
					mUnits.Add(new IfcConversionBasedUnit(IfcUnitEnum.TIMEUNIT, "DAY", mu).mIndex);
				}
				else
					mUnits.Add(seconds.mIndex);
			}
		}
		internal bool mStructuralSet = false;
		internal void setStructuralUnits()
		{
			if (mStructuralSet)
				return;
			mStructuralSet = true;
			DatabaseIfc m = mDatabase;
			
			IfcNamedUnit fu = Find(IfcUnitEnum.FORCEUNIT);
			if (fu == null)
			{
				fu = new IfcSIUnit(m, IfcUnitEnum.FORCEUNIT, IfcSIPrefix.NONE, IfcSIUnitName.NEWTON);
				mUnits.Add(fu.mIndex);
			}
			if (Find(IfcDerivedUnitEnum.TORQUEUNIT) == null)
				mUnits.Add(new IfcDerivedUnit(new IfcDerivedUnitElement(fu, 1), new IfcDerivedUnitElement(m.mSILength, 1), IfcDerivedUnitEnum.TORQUEUNIT).mIndex);
			if (Find(IfcDerivedUnitEnum.LINEARFORCEUNIT) == null)
				mUnits.Add(new IfcDerivedUnit(new IfcDerivedUnitElement(fu, 1), new IfcDerivedUnitElement(m.mSILength, -1), IfcDerivedUnitEnum.LINEARFORCEUNIT).mIndex);
			if (Find(IfcDerivedUnitEnum.PLANARFORCEUNIT) == null)
				mUnits.Add(new IfcDerivedUnit(new IfcDerivedUnitElement(fu, 1), new IfcDerivedUnitElement(m.mSILength, -2), IfcDerivedUnitEnum.PLANARFORCEUNIT).mIndex);
			if (Find(IfcDerivedUnitEnum.MODULUSOFELASTICITYUNIT) == null)
				mUnits.Add(new IfcDerivedUnit(new IfcDerivedUnitElement(fu, 1), new IfcDerivedUnitElement(m.mSILength, -2), IfcDerivedUnitEnum.MODULUSOFELASTICITYUNIT).mIndex);

			IfcNamedUnit time = Find(IfcUnitEnum.TIMEUNIT);
			if (time == null || Math.Abs(time.getSIFactor() - 1) < mDatabase.Tolerance)
				time = new IfcSIUnit(mDatabase, IfcUnitEnum.TIMEUNIT, IfcSIPrefix.NONE, IfcSIUnitName.SECOND);
			mUnits.Add(new IfcDerivedUnit(new IfcDerivedUnitElement(m.mSILength, 1), new IfcDerivedUnitElement(time, -2), IfcDerivedUnitEnum.ACCELERATIONUNIT).mIndex);
			mUnits.Add(new IfcSIUnit(m, IfcUnitEnum.PRESSUREUNIT, IfcSIPrefix.NONE, IfcSIUnitName.PASCAL).mIndex);
			mUnits.Add(new IfcDerivedUnit(new IfcDerivedUnitElement(m.mSILength, 3), IfcDerivedUnitEnum.SECTIONMODULUSUNIT).mIndex);
			mUnits.Add(new IfcDerivedUnit(new IfcDerivedUnitElement(m.mSILength, 4), IfcDerivedUnitEnum.MOMENTOFINERTIAUNIT).mIndex);
			IfcSIUnit massu = Find(IfcUnitEnum.MASSUNIT) as IfcSIUnit;
			if (massu == null)
			{
				massu = new IfcSIUnit(m, IfcUnitEnum.MASSUNIT, IfcSIPrefix.KILO, IfcSIUnitName.GRAM);
				mUnits.Add(massu.mIndex);
			}
			if (Find(IfcDerivedUnitEnum.MASSDENSITYUNIT) == null)
				mUnits.Add(new IfcDerivedUnit(new IfcDerivedUnitElement(massu, 1), new IfcDerivedUnitElement(m.mSIVolume, -1), IfcDerivedUnitEnum.MASSDENSITYUNIT).mIndex);
			IfcSIUnit kelvin = Find(IfcUnitEnum.THERMODYNAMICTEMPERATUREUNIT) as IfcSIUnit;
			if (kelvin == null)
			{
				kelvin = new IfcSIUnit(m, IfcUnitEnum.THERMODYNAMICTEMPERATUREUNIT, IfcSIPrefix.NONE, IfcSIUnitName.KELVIN);
				mUnits.Add(kelvin.mIndex);
			}
			if (Find(IfcDerivedUnitEnum.THERMALEXPANSIONCOEFFICIENTUNIT) == null)
				mUnits.Add(new IfcDerivedUnit(new IfcDerivedUnitElement(kelvin, -1), IfcDerivedUnitEnum.THERMALEXPANSIONCOEFFICIENTUNIT).mIndex);
			mUnits.Add(new IfcDerivedUnit(new IfcDerivedUnitElement(fu, 1), new IfcDerivedUnitElement(m.mSILength, -1), IfcDerivedUnitEnum.LINEARSTIFFNESSUNIT).mIndex);
			IfcNamedUnit radians = Find(IfcUnitEnum.PLANEANGLEUNIT);
			if (radians == null || Math.Abs(radians.getSIFactor() - 1) < mDatabase.Tolerance)
				radians = new IfcSIUnit(mDatabase, IfcUnitEnum.PLANEANGLEUNIT, IfcSIPrefix.NONE, IfcSIUnitName.RADIAN);
			mUnits.Add(new IfcDerivedUnit(new IfcDerivedUnitElement(fu, 1), new IfcDerivedUnitElement(m.mSILength, 1), new IfcDerivedUnitElement(radians, -1), IfcDerivedUnitEnum.ROTATIONALSTIFFNESSUNIT).mIndex);
		}
		internal void AddUnit(IfcUnit u) { mUnits.Add(u.Index); }
		internal static void parseFields(IfcUnitAssignment a, List<string> arrFields, ref int ipos) { a.mUnits = ParserSTEP.SplitListLinks(arrFields[ipos++]); }
		protected override string BuildString()
		{
			string str = base.BuildString() + ",(";
			if (mUnits.Count > 0)
			{
				str += ParserSTEP.LinkToString(mUnits[0]);
				for (int icounter = 1; icounter < mUnits.Count; icounter++)
					str += "," + ParserSTEP.LinkToString(mUnits[icounter]);
			}
			return str + ")";
		}
		internal static IfcUnitAssignment Parse(string strDef) { IfcUnitAssignment a = new IfcUnitAssignment(); int ipos = 0; parseFields(a, ParserSTEP.SplitLineFields(strDef), ref ipos); return a; }
		internal IfcNamedUnit Find(IfcUnitEnum unit)
		{
			List<IfcUnit> units = Units;
			foreach (IfcUnit u in units)
			{
				IfcNamedUnit nu = u as IfcNamedUnit;
				if (nu != null && nu.UnitType == unit)
					return nu;
			}
			return null;
		}
		internal IfcDerivedUnit Find(IfcDerivedUnitEnum unit)
		{
			List<IfcUnit> units = Units;
			foreach (IfcUnit u in units)
			{
				IfcDerivedUnit du = u as IfcDerivedUnit;
				if (du != null && du.UnitType == unit)
					return du;
			}
			return null;
		}
		internal void Replace(IfcConversionBasedUnit cbu)
		{
			for (int icounter = 0; icounter < mUnits.Count; icounter++)
			{
				IfcNamedUnit u = mDatabase.mIfcObjects[mUnits[icounter]] as IfcNamedUnit;
				if (u != null && u.UnitType == cbu.UnitType)
				{
					mUnits[icounter] = cbu.mIndex;
					return;
				}
			}
		}
		internal double LengthScaleSI
		{
			get
			{
				List<IfcUnit> units = Units;
#if(REVIT)
			double result = GGYM.Units.MetreToFeet;
#else
				double result = 1;
#endif
				foreach (IfcUnit u in units)
				{
					IfcNamedUnit nu = u as IfcNamedUnit;
					if (nu != null)
					{
						if (nu.UnitType == IfcUnitEnum.LENGTHUNIT)
						{
							double d = nu.getSIFactor();
							mDatabase.ScaleSI = d;
#if(GGRHINOIFC)
							if (GGYM.ggAssembly.mOptions.RhinoDocTolerance)
							{
								double docScale = Rhino.RhinoMath.UnitScale(Rhino.UnitSystem.Meters, Rhino.RhinoDoc.ActiveDoc.ModelUnitSystem);
								mDatabase.Tolerance = docScale * Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance * d;
							}
#endif
#if(REVIT)
						d = mDatabase.mRevitScale = GGYM.Units.MetreToFeet * d; 
#endif
							result = d;
						}
						if (nu.UnitType == IfcUnitEnum.PLANEANGLEUNIT)
						{
							double d = nu.getSIFactor();
							mDatabase.mPlaneAngleToRadians = d;
						}
					}
					else
					{
						IfcDerivedUnit du = u as IfcDerivedUnit;
						if (du != null)
						{

						}
					}
				}
				return result;
			}
		}
		internal double getScaleSI(IfcUnitEnum unitType)
		{
			List<IfcUnit> units = Units;
			foreach (IfcUnit u in units)
			{
				IfcNamedUnit nu = u as IfcNamedUnit;
				if (nu != null && nu.UnitType == unitType)
				{
					IfcSIUnit si = nu as IfcSIUnit;
					if (si != null)
						return si.getSIFactor();
					IfcConversionBasedUnit cbu = nu as IfcConversionBasedUnit;
					if (cbu != null)
					{
						return cbu.getSIFactor();

					}
				}

			}
			return 1;
		}
		internal double getScaleSI(IfcDerivedUnitEnum unitType)
		{
			List<IfcUnit> units = Units;
			foreach (IfcUnit u in units)
			{
				IfcDerivedUnit du = u as IfcDerivedUnit;
				if (du != null)
				{

				}
			}
			return 1;
		}
	}
	public partial class IfcUShapeProfileDef : IfcParameterizedProfileDef
	{
		public override string KeyWord { get { return mKW; } }
		internal static string mKW = "IFCUSHAPEPROFILEDEF";
		internal double mDepth, mFlangeWidth, mWebThickness, mFlangeThickness;// : IfcPositiveLengthMeasure;
		internal double mFilletRadius, mEdgeRadius, mFlangeSlope;// : OPTIONAL IfcPlaneAngleMeasure;
		//internal double mCentreOfGravityInX;// : : OPTIONAL IfcPositiveLengthMeasure // DELETED IFC4 	Superseded by respective attribute of IfcStructuralProfileProperties 
		internal IfcUShapeProfileDef() : base() { }
		internal IfcUShapeProfileDef(IfcUShapeProfileDef p)
			: base(p)
		{
			mDepth = p.mDepth;
			mFlangeWidth = p.mFlangeWidth;
			mWebThickness = p.mWebThickness;
			mFlangeThickness = p.mFlangeThickness;
			mFilletRadius = p.mFilletRadius;
			mEdgeRadius = p.mEdgeRadius;
			mFlangeSlope = p.mFlangeSlope;
		}
		public IfcUShapeProfileDef(DatabaseIfc m, string name, double depth, double flangeWidth, double webThickness, double flangeThickness, double filletRadius, double edgeRadius, double flangeSlope)
			: base(m) { Name = name; mDepth = depth; mFlangeWidth = flangeWidth; mWebThickness = webThickness; mFlangeThickness = flangeThickness; mFilletRadius = filletRadius; mEdgeRadius = edgeRadius; mFlangeSlope = flangeSlope; }
		internal static void parseFields(IfcUShapeProfileDef p, List<string> arrFields, ref int ipos,Schema schema)
		{
			IfcParameterizedProfileDef.parseFields(p, arrFields, ref ipos);
			p.mDepth = ParserSTEP.ParseDouble(arrFields[ipos++]);
			p.mFlangeWidth = ParserSTEP.ParseDouble(arrFields[ipos++]);
			p.mWebThickness = ParserSTEP.ParseDouble(arrFields[ipos++]);
			p.mFlangeThickness = ParserSTEP.ParseDouble(arrFields[ipos++]);
			p.mFilletRadius = ParserSTEP.ParseDouble(arrFields[ipos++]);
			p.mEdgeRadius = ParserSTEP.ParseDouble(arrFields[ipos++]);
			p.mFlangeSlope = ParserSTEP.ParseDouble(arrFields[ipos++]);
			if (schema == Schema.IFC2x3)
				ipos++;
		}
		internal new static IfcUShapeProfileDef Parse(string strDef, Schema schema) { IfcUShapeProfileDef p = new IfcUShapeProfileDef(); int ipos = 0; parseFields(p, ParserSTEP.SplitLineFields(strDef), ref ipos,schema); return p; }
		protected override string BuildString() { return base.BuildString() + "," + ParserSTEP.DoubleToString(mDepth) + "," + ParserSTEP.DoubleToString(mFlangeWidth) + "," + ParserSTEP.DoubleToString(mWebThickness) + "," + ParserSTEP.DoubleToString(mFlangeThickness) + "," + ParserSTEP.DoubleOptionalToString(mFilletRadius) + "," + ParserSTEP.DoubleOptionalToString(mEdgeRadius) + "," + ParserSTEP.DoubleOptionalToString(mFlangeSlope) + (mDatabase.mSchema == Schema.IFC2x3 ? ",$" : ""); }
	}
}
