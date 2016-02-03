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
using GGYM.STEP;

namespace GGYM.IFC
{
	public class IfcDamper : IfcFlowController //IFC4
	{
		internal IfcDamperTypeEnum mPredefinedType = IfcDamperTypeEnum.NOTDEFINED;// OPTIONAL : IfcDamperTypeEnum;
		public IfcDamperTypeEnum PredefinedType { get { return mPredefinedType; } set { mPredefinedType = value; } }

		internal IfcDamper() : base() { }
		internal IfcDamper(IfcDamper b) : base(b) { mPredefinedType = b.mPredefinedType; }
		internal IfcDamper(IfcProduct host, IfcObjectPlacement placement, IfcProductRepresentation representation, IfcDistributionSystem system) : base(host, placement, representation, system) { }

		internal static void parseFields(IfcDamper s, List<string> arrFields, ref int ipos)
		{
			IfcEnergyConversionDevice.parseFields(s, arrFields, ref ipos);
			string str = arrFields[ipos++];
			if (str[0] == '.')
				s.mPredefinedType = (IfcDamperTypeEnum)Enum.Parse(typeof(IfcDamperTypeEnum), str);
		}
		internal new static IfcDamper Parse(string strDef) { IfcDamper s = new IfcDamper(); int ipos = 0; parseFields(s, ParserSTEP.SplitLineFields(strDef), ref ipos); return s; }
		protected override string BuildString()
		{
			return base.BuildString() + (mDatabase.mSchema == Schema.IFC2x3 ? "" : (mPredefinedType == IfcDamperTypeEnum.NOTDEFINED ? ",$" : ",." + mPredefinedType.ToString() + "."));
		}
	}
	public class IfcDamperType : IfcFlowControllerType
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDAMPERTYPE";
		internal IfcDamperTypeEnum mPredefinedType = IfcDamperTypeEnum.NOTDEFINED;// : IfcDamperTypeEnum;
		public IfcDamperTypeEnum PredefinedType { get { return mPredefinedType; } set { mPredefinedType = value; } }
		internal IfcDamperType() : base() { }
		internal IfcDamperType(IfcDamperType t) : base(t) { mPredefinedType = t.mPredefinedType; }
		internal IfcDamperType(DatabaseIfc m, string name, IfcDamperTypeEnum t) : base(m) { Name = name; mPredefinedType = t; }
		internal static void parseFields(IfcDamperType t, List<string> arrFields, ref int ipos) { IfcFlowControllerType.parseFields(t, arrFields, ref ipos); t.mPredefinedType = (IfcDamperTypeEnum)Enum.Parse(typeof(IfcDamperTypeEnum), arrFields[ipos++].Replace(".", "")); }
		internal new static IfcDamperType Parse(string strDef) { IfcDamperType t = new IfcDamperType(); int ipos = 0; parseFields(t, ParserSTEP.SplitLineFields(strDef), ref ipos); return t; }
		protected override string BuildString() { return base.BuildString() + ",." + mPredefinedType.ToString() + "."; }
	}
	public class IfcDate
	{
		internal string mDate = "$";
		internal IfcDate(DateTime datetime) { mDate = convert(datetime); }
		public override string ToString() { return mDate; }
		internal static string convert(DateTime date) { return "'" + date.Year + (date.Month < 10 ? "-0" : "-") + date.Month + (date.Day < 10 ? "-0" : "-") + date.Day + "'"; }
		internal static DateTime convert(string date) { return new DateTime(int.Parse(date.Substring(0, 4)), int.Parse(date.Substring(5, 2)), int.Parse(date.Substring(8, 2))); }
	}
	internal class IfcDateAndTime : BaseClassIfc, IfcDateTimeSelect // DEPRECEATED IFC4
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDATEANDTIME";
		internal int mDateComponent;// : IfcCalendarDate;
		internal int mTimeComponent;// : IfcLocalTime;
		internal IfcDateAndTime(IfcDateAndTime v) : base() { mDateComponent = v.mDateComponent; mTimeComponent = v.mTimeComponent; }
		internal IfcDateAndTime() : base() { }
		internal IfcDateAndTime(DatabaseIfc m, IfcCalendarDate d, IfcLocalTime t, List<int> genData) : base(m) { mDateComponent = d.mIndex; mTimeComponent = t.mIndex; genData.Add(mIndex); }
		internal static void parseFields(IfcDateAndTime d, List<string> arrFields, ref int ipos) { d.mDateComponent = ParserSTEP.ParseLink(arrFields[ipos++]); d.mTimeComponent = ParserSTEP.ParseLink(arrFields[ipos++]); }
		internal static IfcDateAndTime Parse(string strDef) { IfcDateAndTime d = new IfcDateAndTime(); int ipos = 0; parseFields(d, ParserSTEP.SplitLineFields(strDef), ref ipos); return d; }
		protected override string BuildString() { return base.BuildString() + "," + ParserSTEP.LinkToString(mDateComponent) + "," + ParserSTEP.LinkToString(mTimeComponent); }
		public DateTime DateTime
		{
			get
			{
				IfcCalendarDate cd = mDatabase.mIfcObjects[mDateComponent] as IfcCalendarDate;
				IfcLocalTime lt = mDatabase.mIfcObjects[mTimeComponent] as IfcLocalTime;
				return new DateTime(cd.mYearComponent, cd.mMonthComponent, cd.mDayComponent, lt.mHourComponent, lt.mMinuteComponent, (int)lt.mSecondComponent);
			}
		}
	}
	public class IfcDateTime
	{
		internal string mDateTime = "$";
		//	internal IfcDateTime(DateTime datetime) { mDateTime = Convert(datetime); }
		public override string ToString() { return mDateTime; }
		internal static string Convert(DateTime date) { return date.Year + (date.Month < 10 ? "-0" : "-") + date.Month + (date.Day < 10 ? "-0" : "-") + date.Day + (date.Hour < 10 ? "T0" : "T") + date.Hour + (date.Minute < 10 ? ":0" : ":") + date.Minute + (date.Second < 10 ? ":0" : ":") + date.Second; }
		internal static DateTime Convert(string value)
		{
			if (string.IsNullOrEmpty(value) || value == "$")
				return DateTime.MinValue;
			try
			{
				int year = int.Parse(value.Substring(0, 4)), month = int.Parse(value.Substring(5, 2)), day = int.Parse(value.Substring(8, 2));
				if (value.Contains("T"))
				{
					int hour = int.Parse(value.Substring(11, 2)), min = int.Parse(value.Substring(14, 2));
					double seconds = double.Parse(value.Substring(17, value.Length - 17));
					return new DateTime(year, month, day, hour, min, (int)seconds);
				}
				return new DateTime(year, month, day);
			}
			catch (Exception) { }
			DateTime result = DateTime.MinValue;
			return (DateTime.TryParse(value, out result) ? result : DateTime.MinValue);


		}
		internal static IfcDateTimeSelect convertDateTimeSelect(DatabaseIfc m, DateTime date, List<int> genData)
		{
			IfcCalendarDate cd = new IfcCalendarDate(m, date.Day, date.Month, date.Year, genData);
			if (date.Hour + date.Minute + date.Second < m.Tolerance)
				return cd;
			return new IfcDateAndTime(m, cd, new IfcLocalTime(m, date.Hour, date.Minute, date.Second, null, 0, genData), genData);
		}
	}
	internal interface IfcDateTimeSelect : IfcInterface { DateTime DateTime { get; } } // IFC4 IfcCalenderDate, IfcDateAndTime, IfcLocalTime removed.  Date, IfcDateTime, IfcDescriptiveDate, IfcTime added
	//ENTITY IfcDefinedSymbol  // DEPRECEATED IFC4
	public interface IfcDefinitionSelect : IfcInterface { IfcRelDeclares HasContext { get; set; } List<IfcRelAssociates> HasAssociations { get; } DatabaseIfc Model { get; } } // IFC4 SELECT ( IfcObjectDefinition,  IfcPropertyDefinition);
	public class IfcDerivedUnit : BaseClassIfc, IfcUnit
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDERIVEDUNIT";

		private List<int> mElements = new List<int>();// : SET [1:?] OF IfcDerivedUnitElement;
		private IfcDerivedUnitEnum mUnitType;// : IfcDerivedUnitEnum;
		private string mUserDefinedType = "$";// : OPTIONAL IfcLabel;

		internal List<IfcDerivedUnitElement> Elements { get { return mElements.ConvertAll(x => mDatabase.mIfcObjects[x] as IfcDerivedUnitElement); } }
		internal IfcDerivedUnitEnum UnitType { get { return mUnitType; } }
		//internal string UserDefinedType { get { } }

		internal IfcDerivedUnit(IfcDerivedUnit v) : base() { mElements = new List<int>(v.mElements.ToArray()); mUnitType = v.mUnitType; mUserDefinedType = v.mUserDefinedType; }
		internal IfcDerivedUnit() : base() { }
		internal IfcDerivedUnit(IfcDerivedUnitElement element, IfcDerivedUnitEnum type, List<int> genData) : base(element.mDatabase) { mElements.Add(element.mIndex); mUnitType = type; genData.Add(mIndex); }
		internal IfcDerivedUnit(List<IfcDerivedUnitElement> elements, IfcDerivedUnitEnum type, List<int> genData) : base(elements[0].mDatabase) { mElements = elements.ConvertAll(x => x.mIndex); mUnitType = type; genData.Add(mIndex); }
		internal IfcDerivedUnit(IfcDerivedUnitElement due1, IfcDerivedUnitElement due2, IfcDerivedUnitEnum type, List<int> genData) : base(due1.mDatabase) { mElements.Add(due1.mIndex); mElements.Add(due2.mIndex); mUnitType = type; genData.Add(mIndex); }
		internal IfcDerivedUnit(IfcDerivedUnitElement due1, IfcDerivedUnitElement due2, IfcDerivedUnitElement due3, IfcDerivedUnitEnum type, List<int> genData) : base(due1.mDatabase) { mElements.Add(due1.mIndex); mElements.Add(due2.mIndex); mElements.Add(due3.mIndex); mUnitType = type; genData.Add(mIndex); }
		internal static void parseFields(IfcDerivedUnit u, List<string> arrFields, ref int ipos) { u.mElements = ParserSTEP.SplitListLinks(arrFields[ipos++]); u.mUnitType = (IfcDerivedUnitEnum)Enum.Parse(typeof(IfcDerivedUnitEnum), arrFields[ipos++].Replace(".", "")); u.mUserDefinedType = arrFields[ipos++]; }
		internal static IfcDerivedUnit Parse(string strDef) { IfcDerivedUnit u = new IfcDerivedUnit(); int ipos = 0; parseFields(u, ParserSTEP.SplitLineFields(strDef), ref ipos); return u; }
		protected override string BuildString()
		{
			string str = base.BuildString() + ",(" + ParserSTEP.LinkToString(mElements[0]);
			for (int icounter = 1; icounter < mElements.Count; icounter++)
				str += "," + ParserSTEP.LinkToString(mElements[icounter]);
			return str + "),." + mUnitType.ToString() + ".," + mUserDefinedType;
		}
		internal double getSIFactor()
		{
			List<IfcDerivedUnitElement> elements = Elements;
			double result = 1;
			foreach (IfcDerivedUnitElement due in elements)
				result *= Math.Pow(due.Unit.getSIFactor(), due.Exponent);
			return result;
		}
	}
	public class IfcDerivedUnitElement : BaseClassIfc
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDERIVEDUNITELEMENT";

		private int mUnit;// : IfcNamedUnit;
		private int mExponent;// : INTEGER;

		internal IfcNamedUnit Unit { get { return mDatabase.mIfcObjects[mUnit] as IfcNamedUnit; } }
		internal int Exponent { get { return mExponent; } }

		internal IfcDerivedUnitElement(IfcDerivedUnitElement v) : base() { mUnit = v.mUnit; mExponent = v.mExponent; }
		internal IfcDerivedUnitElement() : base() { }
		internal IfcDerivedUnitElement(IfcNamedUnit u, int exponent, List<int> genData) : base(u.mDatabase) { mUnit = u.mIndex; mExponent = exponent; genData.Add(mIndex); }
		internal static void parseFields(IfcDerivedUnitElement e, List<string> arrFields, ref int ipos) { e.mUnit = ParserSTEP.ParseLink(arrFields[ipos++]); e.mExponent = ParserSTEP.ParseInt(arrFields[ipos++]); }
		internal static IfcDerivedUnitElement Parse(string strDef) { IfcDerivedUnitElement e = new IfcDerivedUnitElement(); int ipos = 0; parseFields(e, ParserSTEP.SplitLineFields(strDef), ref ipos); return e; }
		protected override string BuildString() { return base.BuildString() + "," + ParserSTEP.LinkToString(mUnit) + "," + ParserSTEP.IntToString(mExponent); }
	}
	public partial class IfcDerivedProfileDef : IfcProfileDef
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDERIVEDPROFILEDEF";
		private int mContainerProfile;// : IfcProfileDef;
		private int mOperator;// : IfcCartesianTransformationOperator2D;
		internal string mLabel = "$";// : OPTIONAL IfcLabel;

		internal IfcProfileDef ContainerProfile { get { return mDatabase.mIfcObjects[mContainerProfile] as IfcProfileDef; } }
		internal IfcCartesianTransformationOperator2D Operator { get { return mDatabase.mIfcObjects[mOperator] as IfcCartesianTransformationOperator2D; } }

		internal IfcDerivedProfileDef() : base() { }
		internal IfcDerivedProfileDef(IfcDerivedProfileDef el) : base(el) { mContainerProfile = el.mContainerProfile; mOperator = el.mOperator; mLabel = el.mLabel; }
		
		internal static void parseFields(IfcDerivedProfileDef p, List<string> arrFields, ref int ipos) { IfcProfileDef.parseFields(p, arrFields, ref ipos); p.mContainerProfile = ParserSTEP.ParseLink(arrFields[ipos++]); p.mOperator = ParserSTEP.ParseLink(arrFields[ipos++]); p.mLabel = arrFields[ipos++].Replace("'", ""); }
		internal new static IfcDerivedProfileDef Parse(string strDef) { int ipos = 0; IfcDerivedProfileDef p = new IfcDerivedProfileDef(); parseFields(p, ParserSTEP.SplitLineFields(strDef), ref ipos); return p; }
		protected override string BuildString() { return base.BuildString() + "," + ParserSTEP.LinkToString(mContainerProfile) + "," + ParserSTEP.LinkToString(mOperator) + (mLabel == "$" ? ",$" : ",'" + mLabel + "'"); }

	}
	public class IfcDiameterDimension : IfcDimensionCurveDirectedCallout // DEPRECEATED IFC4
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDIAMETERDIMENSION";
		internal IfcDiameterDimension() : base() { }
		internal IfcDiameterDimension(IfcDiameterDimension el) : base(el) { }
		internal new static IfcDiameterDimension Parse(string strDef) { IfcDiameterDimension d = new IfcDiameterDimension(); int ipos = 0; parseFields(d, ParserSTEP.SplitLineFields(strDef), ref ipos); return d; }
		internal static void parseFields(IfcDiameterDimension d, List<string> arrFields, ref int ipos) { IfcDimensionCurveDirectedCallout.parseFields(d, arrFields, ref ipos); }
	}
	public class IfcDimensionalExponents : BaseClassIfc
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDIMENSIONALEXPONENTS";
		internal int mLengthExponent;// : INTEGER;
		internal int mMassExponent;// : INTEGER;
		internal int mTimeExponent;// : INTEGER;
		internal int mElectricCurrentExponent;// : INTEGER;
		internal int mThermodynamicTemperatureExponent;// : INTEGER;
		internal int mAmountOfSubstanceExponent;// : INTEGER;
		internal int mLuminousIntensityExponent;// : INTEGER;
		internal IfcDimensionalExponents() : base() { }
		internal IfcDimensionalExponents(IfcDimensionalExponents v)
			: base()
		{
			mLengthExponent = v.mLengthExponent;
			mMassExponent = v.mMassExponent;
			mTimeExponent = v.mTimeExponent;
			mElectricCurrentExponent = v.mElectricCurrentExponent;
			mThermodynamicTemperatureExponent = v.mThermodynamicTemperatureExponent;
			mAmountOfSubstanceExponent = v.mAmountOfSubstanceExponent;
			mLuminousIntensityExponent = v.mLuminousIntensityExponent;
		}
		internal IfcDimensionalExponents(DatabaseIfc m, int len, int mass, int time, int elecCurr, int themrmo, int amountSubs, int luminous, List<int> genData)
			: base(m)
		{
			mLengthExponent = len;
			mMassExponent = mass;
			mTimeExponent = time;
			mElectricCurrentExponent = elecCurr;
			mThermodynamicTemperatureExponent = themrmo;
			mAmountOfSubstanceExponent = amountSubs;
			mLuminousIntensityExponent = luminous;
			genData.Add(mIndex);
		}
		internal static void parseFields(IfcDimensionalExponents e, List<string> arrFields, ref int ipos)
		{
			e.mLengthExponent = int.Parse(arrFields[ipos++]);
			e.mMassExponent = int.Parse(arrFields[ipos++]);
			e.mTimeExponent = int.Parse(arrFields[ipos++]);
			e.mElectricCurrentExponent = int.Parse(arrFields[ipos++]);
			e.mThermodynamicTemperatureExponent = int.Parse(arrFields[ipos++]);
			e.mAmountOfSubstanceExponent = int.Parse(arrFields[ipos++]);
			e.mLuminousIntensityExponent = int.Parse(arrFields[ipos++]);
		}
		internal static IfcDimensionalExponents Parse(string strDef) { IfcDimensionalExponents e = new IfcDimensionalExponents(); int ipos = 0; parseFields(e, ParserSTEP.SplitLineFields(strDef), ref ipos); return e; }
		protected override string BuildString() { return base.BuildString() + "," + mLengthExponent + "," + mMassExponent + "," + mTimeExponent + "," + mElectricCurrentExponent + "," + mThermodynamicTemperatureExponent + "," + mAmountOfSubstanceExponent + "," + mLuminousIntensityExponent; }
	}
	public class IfcDimensionCalloutRelationship : IfcDraughtingCalloutRelationship // DEPRECEATED IFC4
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDIMENSIONCALLOUTRELATIONSHIP";
		internal IfcDimensionCalloutRelationship() : base() { }
		internal IfcDimensionCalloutRelationship(IfcDimensionCalloutRelationship i) : base(i) { }
		internal new static IfcDimensionCalloutRelationship Parse(string strDef) { IfcDimensionCalloutRelationship r = new IfcDimensionCalloutRelationship(); int ipos = 0; parseFields(r, ParserSTEP.SplitLineFields(strDef), ref ipos); return r; }
		internal static void parseFields(IfcDimensionCalloutRelationship r, List<string> arrFields, ref int ipos) { IfcDraughtingCalloutRelationship.parseFields(r, arrFields, ref ipos); }
	}
	public class IfcDimensionCurve : IfcAnnotationCurveOccurrence // DEPRECEATED IFC4
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDIMENSIONCURVE";
		internal List<int> mAnnotatedBySymbols = new List<int>();// SET [0:2] OF IfcTerminatorSymbol FOR AnnotatedCurve; 
		internal IfcDimensionCurve() : base() { }
		internal IfcDimensionCurve(IfcDimensionCurve p) : base(p) { mAnnotatedBySymbols = new List<int>(p.mAnnotatedBySymbols.ToArray()); }
		internal static void parseFields(IfcDimensionCurve fs, List<string> arrFields, ref int ipos) { IfcAnnotationCurveOccurrence.parseFields(fs, arrFields, ref ipos); fs.mAnnotatedBySymbols = ParserSTEP.SplitListLinks(arrFields[ipos++]); }
		internal new static IfcDimensionCurve Parse(string strDef) { IfcDimensionCurve d = new IfcDimensionCurve(); int ipos = 0; parseFields(d, ParserSTEP.SplitLineFields(strDef), ref ipos); return d; }
		protected override string BuildString()
		{
			string str = base.BuildString() + ",(";
			if (mAnnotatedBySymbols.Count > 0)
			{
				str += ParserSTEP.LinkToString(mAnnotatedBySymbols[0]);
				for (int icounter = 1; icounter < mAnnotatedBySymbols.Count; icounter++)
					str += "," + ParserSTEP.LinkToString(mAnnotatedBySymbols[icounter]);
			}
			return str + "}";
		}
	}
	public class IfcDimensionCurveDirectedCallout : IfcDraughtingCallout // DEPRECEATED IFC4 SUPERTYPE OF (ONEOF (IfcAngularDimension ,IfcDiameterDimension ,IfcLinearDimension ,IfcRadiusDimension))
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDIMENSIONCURVEDIRECTEDCALLOUT";
		internal IfcDimensionCurveDirectedCallout() : base() { }
		internal IfcDimensionCurveDirectedCallout(IfcDimensionCurveDirectedCallout el) : base(el) { }
		internal new static IfcDimensionCurveDirectedCallout Parse(string strDef) { IfcDimensionCurveDirectedCallout d = new IfcDimensionCurveDirectedCallout(); int ipos = 0; parseFields(d, ParserSTEP.SplitLineFields(strDef), ref ipos); return d; }
		internal static void parseFields(IfcDimensionCurveDirectedCallout d, List<string> arrFields, ref int ipos) { IfcDraughtingCallout.parseFields(d, arrFields, ref ipos); }
	}
	public class IfcDimensionCurveTerminator : IfcTerminatorSymbol // DEPRECEATED IFC4
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDIMENSIONCURVETERMINATOR";
		internal IfcDimensionExtentUsage mRole;// : IfcDimensionExtentUsage;
		internal IfcDimensionCurveTerminator() : base() { }
		internal IfcDimensionCurveTerminator(IfcDimensionCurveTerminator i) : base(i) { mRole = i.mRole; }
		internal new static IfcDimensionCurveTerminator Parse(string strDef) { IfcDimensionCurveTerminator t = new IfcDimensionCurveTerminator(); int ipos = 0; parseFields(t, ParserSTEP.SplitLineFields(strDef), ref ipos); return t; }
		internal static void parseFields(IfcDimensionCurveTerminator t, List<string> arrFields, ref int ipos) { IfcTerminatorSymbol.parseFields(t, arrFields, ref ipos); t.mRole = (IfcDimensionExtentUsage)Enum.Parse(typeof(IfcDimensionExtentUsage), arrFields[ipos++].Replace(".", "")); }
		protected override string BuildString() { return base.BuildString() + ",." + mRole.ToString() + "."; }
	}
	public class IfcDimensionPair : IfcDraughtingCalloutRelationship // DEPRECEATED IFC4
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDIMENSIONPAIR";
		internal IfcDimensionPair() : base() { }
		internal IfcDimensionPair(IfcDimensionPair i) : base((IfcDraughtingCalloutRelationship)i) { }
		internal new static IfcDimensionPair Parse(string strDef) { IfcDimensionPair d = new IfcDimensionPair(); int ipos = 0; parseFields(d, ParserSTEP.SplitLineFields(strDef), ref ipos); return d; }
		internal static void parseFields(IfcDimensionPair d, List<string> arrFields, ref int ipos) { IfcDraughtingCalloutRelationship.parseFields(d, arrFields, ref ipos); }
	}
	//ENTITY IfcDimensionalExponents;
	public partial class IfcDirection : IfcGeometricRepresentationItem
	{
		private double mCoordinateX = 0, mCoordinateY = 0, mCoordinateZ = 0;

		public double CoordinateX { get { return mCoordinateX; } set { mCoordinateX = value; } }
		public double CoordinateY { get { return mCoordinateY; } set { mCoordinateY = value; } }
		public double CoordinateZ { get { return double.IsNaN( mCoordinateZ) ? 0 : mCoordinateZ; } set { mCoordinateZ = value; } }

		internal IfcDirection() : base() { }
		internal IfcDirection(IfcDirection d) : base(d) { mCoordinateX = d.mCoordinateX; mCoordinateY = d.mCoordinateY; mCoordinateZ = d.mCoordinateZ; }
		public IfcDirection(DatabaseIfc m, double x, double y) : base(m) { mCoordinateX = x; mCoordinateY = y; mCoordinateZ = double.NaN; }
		public IfcDirection(DatabaseIfc m, double x, double y, double z) : base(m) { mCoordinateX = x; mCoordinateY = y; mCoordinateZ = z; }

		internal static void parseFields(IfcDirection d, List<string> arrFields, ref int ipos)
		{
			IfcGeometricRepresentationItem.parseFields(d, arrFields, ref ipos);
			//int count = arrFields.Count;
			string s = arrFields[ipos++];
			List<string> arrCoords = ParserSTEP.SplitLineFields(s.Substring(1, s.Length - 2));
			if (arrCoords.Count > 0)
			{
				d.mCoordinateX = ParserSTEP.ParseDouble(arrCoords[0]);
				if (arrCoords.Count > 1)
				{
					d.mCoordinateY = ParserSTEP.ParseDouble(arrCoords[1]);
					d.mCoordinateZ = (arrCoords.Count > 2 ? ParserSTEP.ParseDouble(arrCoords[2]) : double.NaN);
				}
			}
		}
		internal static IfcDirection Parse(string strDef) { IfcDirection d = new IfcDirection(); int ipos = 0; parseFields(d, ParserSTEP.SplitLineFields(strDef), ref ipos); return d; }
		protected override string BuildString()
		{ 
			return base.BuildString() + ",(" + ParserSTEP.DoubleToString(Math.Round(mCoordinateX, 8)) + "," +
				ParserSTEP.DoubleToString(Math.Round(mCoordinateY, 8)) + (double.IsNaN(mCoordinateZ) ? "" : "," + ParserSTEP.DoubleToString(Math.Round(mCoordinateZ, 8))) + ")";
		}
	}
	
	public partial class IfcDiscreteAccessory : IfcElementComponent
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDISCRETEACCESSORY";
		internal IfcDiscreteAccessoryTypeEnum mPredefinedType = IfcDiscreteAccessoryTypeEnum.NOTDEFINED;// : OPTIONAL IfcDiscreteAccessoryTypeEnum;
		public IfcDiscreteAccessoryTypeEnum PredefinedType { get { return mPredefinedType; } set { mPredefinedType = value; } }
		internal IfcDiscreteAccessory() : base() { }
		internal IfcDiscreteAccessory(IfcDiscreteAccessory a) : base(a) { mPredefinedType = a.mPredefinedType; }
		internal IfcDiscreteAccessory(IfcProduct host, IfcObjectPlacement p, IfcProductRepresentation r) : base(host, p, r) { }
		 
		internal static IfcDiscreteAccessory Parse(string strDef, Schema schema) { int ipos = 0; IfcDiscreteAccessory a = new IfcDiscreteAccessory(); parseFields(a, ParserSTEP.SplitLineFields(strDef), ref ipos, schema); return a; }
		internal static void parseFields(IfcDiscreteAccessory a, List<string> arrFields, ref int ipos, Schema schema)
		{
			IfcElementComponent.parseFields(a, arrFields, ref ipos);
			if (schema != Schema.IFC2x3)
			{
				string s = arrFields[ipos++];
				if (s.StartsWith("."))
					a.mPredefinedType = (IfcDiscreteAccessoryTypeEnum)Enum.Parse(typeof(IfcDiscreteAccessoryTypeEnum), s.Replace(".", ""));
			}
		}
		protected override string BuildString() { return base.BuildString() + (mDatabase.mSchema == Schema.IFC2x3 ? "" : (mPredefinedType == IfcDiscreteAccessoryTypeEnum.NOTDEFINED ? ",$" : ",." + mPredefinedType + ".")); }
	}
	public partial class IfcDiscreteAccessoryType : IfcElementComponentType
	{
		internal IfcDiscreteAccessoryTypeEnum mPredefinedType = IfcDiscreteAccessoryTypeEnum.NOTDEFINED;//:	OPTIONAL IfcDiscreteAccessoryTypeEnum; IFC4	
		public IfcDiscreteAccessoryTypeEnum PredefinedType { get { return mPredefinedType; } set { mPredefinedType = value; } }
		internal IfcDiscreteAccessoryType() : base() { }
		internal IfcDiscreteAccessoryType(IfcDiscreteAccessoryType t) : base(t) { }
		internal IfcDiscreteAccessoryType(DatabaseIfc m, string name, IfcDiscreteAccessoryTypeEnum type) : base(m) { Name = name; mPredefinedType = type; }
		internal static IfcDiscreteAccessoryType Parse(string strDef, Schema schema) { IfcDiscreteAccessoryType t = new IfcDiscreteAccessoryType(); int ipos = 0; parseFields(t, ParserSTEP.SplitLineFields(strDef), ref ipos, schema); return t; }
		internal static void parseFields(IfcDiscreteAccessoryType t, List<string> arrFields, ref int ipos, Schema schema)
		{
			IfcElementComponentType.parseFields(t, arrFields, ref ipos);
			if (schema != Schema.IFC2x3)
			{
				string str = arrFields[ipos++];
				if (str[0] == '.')
					t.mPredefinedType = (IfcDiscreteAccessoryTypeEnum)Enum.Parse(typeof(IfcDiscreteAccessoryTypeEnum), str.Replace(".", ""));
			}
		}
		protected override string BuildString() { return base.BuildString() + (mDatabase.mSchema == Schema.IFC2x3 ? "" : (mPredefinedType == IfcDiscreteAccessoryTypeEnum.NOTDEFINED ? ",$" : ",." + mPredefinedType + ".")); }
	}
	public class IfcDistributionChamberElement : IfcDistributionFlowElement
	{
		internal IfcDistributionChamberElementTypeEnum mPredefinedType = IfcDistributionChamberElementTypeEnum.NOTDEFINED;//	:	OPTIONAL IfcDistributionChamberElementTypeEnum;
		public IfcDistributionChamberElementTypeEnum PredefinedType { get { return mPredefinedType; } set { mPredefinedType = value; } }

		internal IfcDistributionChamberElement() : base() { }
		internal IfcDistributionChamberElement(IfcDistributionChamberElement e) : base(e) { }
		internal IfcDistributionChamberElement(IfcProduct host, IfcObjectPlacement placement, IfcProductRepresentation representation, IfcDistributionSystem system) : base(host, placement, representation, system) { }

		internal static void parseFields(IfcDistributionChamberElement e, List<string> arrFields, ref int ipos, Schema schema)
		{
			IfcDistributionFlowElement.parseFields(e, arrFields, ref ipos);
			if (schema != Schema.IFC2x3)
			{
				string str = arrFields[ipos++];
				if (str[0] == '.')
					e.mPredefinedType = (IfcDistributionChamberElementTypeEnum)Enum.Parse(typeof(IfcDistributionChamberElementTypeEnum), str);
			}
		}
		internal new static IfcDistributionChamberElement Parse(string strDef, Schema schema) { IfcDistributionChamberElement e = new IfcDistributionChamberElement(); int ipos = 0; parseFields(e, ParserSTEP.SplitLineFields(strDef), ref ipos, schema); return e; }
		protected override string BuildString()
		{
			return base.BuildString() + (mDatabase.mSchema == Schema.IFC2x3 ? "" : (mPredefinedType == IfcDistributionChamberElementTypeEnum.NOTDEFINED ? ",$" : ",." + mPredefinedType + "."));
		}
	}
	public class IfcDistributionChamberElementType : IfcDistributionFlowElementType
	{
		internal IfcDistributionChamberElementTypeEnum mPredefinedType;
		public IfcDistributionChamberElementTypeEnum PredefinedType { get { return mPredefinedType; } set { mPredefinedType = value; } }
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDISTRIBUTIONCHAMBERELEMENTTYPE";
		internal IfcDistributionChamberElementType() : base() { }
		internal IfcDistributionChamberElementType(IfcDistributionChamberElementType t) : base(t) { mPredefinedType = t.mPredefinedType; }
		internal static void parseFields(IfcDistributionChamberElementType t, List<string> arrFields, ref int ipos) { IfcDistributionFlowElementType.parseFields(t, arrFields, ref ipos); t.mPredefinedType = (IfcDistributionChamberElementTypeEnum)Enum.Parse(typeof(IfcDistributionChamberElementTypeEnum), arrFields[ipos++].Replace(".", "")); }
		internal new static IfcDistributionChamberElementType Parse(string strDef) { IfcDistributionChamberElementType t = new IfcDistributionChamberElementType(); int ipos = 0; parseFields(t, ParserSTEP.SplitLineFields(strDef), ref ipos); return t; }
		protected override string BuildString() { return base.BuildString() + ",." + mPredefinedType.ToString() + "."; }
	}
	public class IfcDistributionCircuit : IfcDistributionSystem
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDISTRIBUTIONCIRCUIT";
		internal IfcDistributionCircuit() : base() { }
		internal IfcDistributionCircuit(IfcDistributionCircuit i) : base(i) { }
		internal IfcDistributionCircuit(IfcSpatialElement bldg, string name, string longname, IfcDistributionSystemEnum type, List<int> genData) : base(bldg, name, longname, type, genData) { }
		internal new static IfcDistributionCircuit Parse(string strDef) { IfcDistributionCircuit m = new IfcDistributionCircuit(); int ipos = 0; parseFields(m, ParserSTEP.SplitLineFields(strDef), ref ipos); return m; }
		internal static void parseFields(IfcDistributionCircuit c, List<string> arrFields, ref int ipos) { IfcDistributionSystem.parseFields(c, arrFields, ref ipos); }
	}
	public class IfcDistributionControlElement : IfcDistributionElement // SUPERTYPE OF(ONEOF(IfcActuator, IfcAlarm, IfcController,
	{ // IfcFlowInstrument, IfcProtectiveDeviceTrippingUnit, IfcSensor, IfcUnitaryControlElement)) //"IFCDISTRIBUTIONCONTROLELEMENT"
		public override string KeyWord { get { return mDatabase.mSchema == Schema.IFC2x3 ? "IFCDISTRIBUTIONCONTROLELEMENT" : base.KeyWord; } }

		internal new enum SubTypes { IfcActuator, IfcAlarm, IfcController, IfcFlowInstrument, IfcProtectiveDeviceTrippingUnit, IfcSensor, IfcUnitaryControlElement }
		internal string mControlElementId = "$";// : OPTIONAL IfcIdentifier;
		internal string ControlElementId { get { return (mControlElementId == "$" ? "" : ParserIfc.Decode(mControlElementId)); } set { mControlElementId = (string.IsNullOrEmpty(value) ? "$" : ParserIfc.Encode(value.Replace("'", ""))); } }

		internal IfcDistributionControlElement() : base() { }
		internal IfcDistributionControlElement(IfcDistributionControlElement e) : base(e) { }
		internal IfcDistributionControlElement(IfcProduct host, IfcObjectPlacement p, IfcProductRepresentation r, IfcDistributionSystem system) : base(host,p,r, system) { }
		internal static void parseFields(IfcDistributionControlElement e, List<string> arrFields, ref int ipos) { IfcDistributionElement.parseFields(e, arrFields, ref ipos); e.mControlElementId = arrFields[ipos++]; }
		internal new static IfcDistributionControlElement Parse(string strDef) { IfcDistributionControlElement e = new IfcDistributionControlElement(); int ipos = 0; parseFields(e, ParserSTEP.SplitLineFields(strDef), ref ipos); return e; }
		protected override string BuildString() { return base.BuildString() + "," + mControlElementId; }
	}
	public abstract class IfcDistributionControlElementType : IfcDistributionElementType //ABSTRACT SUPERTYPE OF (ONEOF (IfcActuatorType ,IfcAlarmType ,IfcControllerType ,IfcFlowInstrumentType ,IfcSensorType))
	{
		protected IfcDistributionControlElementType() : base() { }
		protected IfcDistributionControlElementType(IfcDistributionControlElementType t) : base(t) { }
		protected IfcDistributionControlElementType(DatabaseIfc m) : base(m) { }
		protected static void parseFields(IfcDistributionControlElementType t, List<string> arrFields, ref int ipos) { IfcDistributionElementType.parseFields(t, arrFields, ref ipos); }
		//internal override IfcElement genMappedItemElement(DatabaseIfc m, Plane pln, ElementParams p, IfcProduct container, IfcDistributionSystem system, List<int> genData, out IfcRepresentationItem i) { return new IfcDistributionControlElement(m, p, this, i = new IfcMappedItem(m, RepresentationMaps[0], new IfcCartesianTransformationOperator3D(m, pln, genData), genData), container,system, genData); }
	}
	public partial class IfcDistributionElement : IfcElement //SUPERTYPE OF (ONEOF (IfcDistributionControlElement ,IfcDistributionFlowElement))
	{
		internal new enum SubTypes { IfcDistributionControlElement, IfcDistributionFlowElement }  

		internal IfcDistributionElement() : base() { }
		protected IfcDistributionElement(IfcDistributionElement be) : base(be) { }
		internal IfcDistributionElement(IfcProduct host, IfcObjectPlacement p, IfcProductRepresentation r) : base(host, p, r) { }
		internal IfcDistributionElement(IfcProduct host, IfcObjectPlacement p, IfcProductRepresentation r, IfcDistributionSystem system) : this(host,p,r) { if (system != null) system.assign(this); }
		
		internal static void parseFields(IfcDiscreteAccessoryType e, List<string> arrFields, ref int ipos) { IfcElement.parseFields(e, arrFields, ref ipos); }
		internal static IfcDistributionElement Parse(string strDef) { IfcDistributionElement e = new IfcDistributionElement(); int ipos = 0; parseFields(e, ParserSTEP.SplitLineFields(strDef), ref ipos); return e; }
		internal IfcDistributionSystem getSystem()
		{
			foreach (IfcRelAssigns ra in mHasAssignments)
			{
				IfcRelAssignsToGroup rag = ra as IfcRelAssignsToGroup;
				if (rag != null)
				{
					IfcDistributionSystem ds = rag.RelatingGroup as IfcDistributionSystem;
					if (ds != null)
						return ds;
				}
			}
			return null;
		}
	}
	public class IfcDistributionElementType : IfcElementType
	{
		internal IfcDistributionElementType() : base() { }
		protected IfcDistributionElementType(IfcDistributionElementType t) : base(t) { }
		protected IfcDistributionElementType(DatabaseIfc m) : base(m) { }
		internal static void parseFields(IfcDistributionElementType t, List<string> arrFields, ref int ipos) { IfcElementType.parseFields(t, arrFields, ref ipos); }
		internal new static IfcDistributionElementType Parse(string strDef) { IfcDistributionElementType t = new IfcDistributionElementType(); int ipos = 0; parseFields(t, ParserSTEP.SplitLineFields(strDef), ref ipos); return t; }
		internal virtual IfcDistributionElement genElement(IfcDistributionPort p1, IfcDistributionPort p2, IfcDistributionSystem system, List<int> genData) { return null; }
	}
	public partial class IfcDistributionFlowElement : IfcDistributionElement //SUPERTYPE OF (ONEOF (IfcDistributionChamberElement ,IfcEnergyConversionDevice ,
	{ 	//IfcFlowController ,IfcFlowFitting ,IfcFlowMovingDevice,IfcFlowSegment ,IfcFlowStorageDevice ,IfcFlowTerminal ,IfcFlowTreatmentDevice))
		internal new enum SubTypes { IfcDistributionChamberElement, IfcEnergyConversionDevice, IfcFlowController, IfcFlowFitting, IfcFlowMovingDevice, IfcFlowSegment, IfcFlowStorageDevice, IfcFlowTerminal, IfcFlowTreatmentDevice }
		
		public override string KeyWord { get { return mDatabase.mSchema == Schema.IFC2x3 ? "IFCDISTRIBUTIONFLOWELEMENT" : base.KeyWord; } }

		//INVERSE 	HasControlElements : SET [0:1] OF IfcRelFlowControlElements FOR RelatingFlowElement;
		//GG
		internal IfcDistributionPort mSourcePort, mSinkPort;

		internal IfcDistributionFlowElement() : base() { }
		internal IfcDistributionFlowElement(IfcDistributionFlowElement e) : base(e) { }
		internal IfcDistributionFlowElement(IfcProduct host, IfcObjectPlacement placement, IfcProductRepresentation representation, IfcDistributionSystem system) : base(host, placement, representation, system) { }
	 
		internal static void parseFields(IfcDistributionFlowElement e, List<string> arrFields, ref int ipos) { IfcDistributionElement.parseFields(e, arrFields, ref ipos); }
		internal new static IfcDistributionFlowElement Parse(string strDef) { IfcDistributionFlowElement e = new IfcDistributionFlowElement(); int ipos = 0; parseFields(e, ParserSTEP.SplitLineFields(strDef), ref ipos); return e; }
	}
	public abstract class IfcDistributionFlowElementType : IfcDistributionElementType //IfcDistributionChamberElementType, IfcEnergyConversionDeviceType, IfcFlowControllerType,
	{ // IfcFlowFittingType, IfcFlowMovingDeviceType, IfcFlowSegmentType, IfcFlowStorageDeviceType, IfcFlowTerminalType, IfcFlowTreatmentDeviceType))
		protected IfcDistributionFlowElementType() : base() { }
		protected IfcDistributionFlowElementType(IfcDistributionFlowElementType t) : base(t) { }
		protected IfcDistributionFlowElementType(DatabaseIfc m) : base(m) { }
		protected static void parseFields(IfcDistributionFlowElementType t, List<string> arrFields, ref int ipos) { IfcDistributionElementType.parseFields(t, arrFields, ref ipos); }
	}
	public partial class IfcDistributionPort : IfcPort
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDISTRIBUTIONPORT";
		internal IfcFlowDirectionEnum mFlowDirection = IfcFlowDirectionEnum.NOTDEFINED;
		internal IfcDistributionPortTypeEnum mPredefinedType = IfcDistributionPortTypeEnum.NOTDEFINED; // IFC4 : OPTIONAL IfcDistributionPortTypeEnum;
		internal IfcDistributionSystemEnum mSystemType = IfcDistributionSystemEnum.NOTDEFINED;// IFC4 : OPTIONAL IfcDistributionSystemEnum;
		internal IfcDistributionPort() : base() { }
		internal IfcDistributionPort(IfcDistributionPort b) : base(b) { mFlowDirection = b.mFlowDirection; }
		internal static void parseFields(IfcDistributionPort p, List<string> arrFields, ref int ipos, Schema schema)
		{
			IfcPort.parseFields(p, arrFields, ref ipos);
			p.mFlowDirection = (IfcFlowDirectionEnum)Enum.Parse(typeof(IfcFlowDirectionEnum), arrFields[ipos++].Replace(".", ""));
			if (schema != Schema.IFC2x3)
			{
				string s = arrFields[ipos++];
				if (s.StartsWith("."))
					p.mPredefinedType = (IfcDistributionPortTypeEnum)Enum.Parse(typeof(IfcDistributionPortTypeEnum), s.Replace(".", ""));
				s = arrFields[ipos++];
				if (s.StartsWith("."))
					p.mSystemType = (IfcDistributionSystemEnum)Enum.Parse(typeof(IfcDistributionSystemEnum), s.Replace(".", ""));
			}
		}
		internal static IfcDistributionPort Parse(string strDef, Schema schema) { IfcDistributionPort p = new IfcDistributionPort(); int ipos = 0; parseFields(p, ParserSTEP.SplitLineFields(strDef), ref ipos, schema); return p; }
		protected override string BuildString() { return base.BuildString() + ",." + mFlowDirection.ToString() + (mDatabase.mSchema == Schema.IFC2x3 ? "." : ".,." + mPredefinedType + ".,." + mSystemType + "."); }
	}
	public class IfcDistributionConduit : IfcDistributionSystem //IFC4
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDISTRIBUTIONCONDUIT";
		internal IfcDistributionConduit() : base() { }
		internal IfcDistributionConduit(IfcDistributionConduit i) : base(i) { }
		internal IfcDistributionConduit(IfcSpatialElement bldg, string name, string longname, IfcDistributionSystemEnum type, List<int> genData) : base(bldg, name, longname, type, genData) { }
		internal new static IfcDistributionConduit Parse(string strDef) { IfcDistributionConduit m = new IfcDistributionConduit(); int ipos = 0; parseFields(m, ParserSTEP.SplitLineFields(strDef), ref ipos); return m; }
	}
	public partial class IfcDistributionSystem : IfcSystem //SUPERTYPE OF(IfcDistributionCircuit) IFC4
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDISTRIBUTIONSYSTEM";
		internal string mLongName = "$"; // 	OPTIONAL IfcLabel
		internal IfcDistributionSystemEnum mPredefinedType = IfcDistributionSystemEnum.NOTDEFINED;// : OPTIONAL IfcDistributionSystemEnum
		internal IfcDistributionSystem() : base() { }
		internal IfcDistributionSystem(IfcDistributionSystem i) : base(i) { mLongName = i.mLongName; mPredefinedType = i.mPredefinedType; }
		internal IfcDistributionSystem(IfcSpatialElement bldg, string name, string longname, IfcDistributionSystemEnum type, List<int> genData) : base(bldg, name, genData) { if (!string.IsNullOrEmpty(longname)) mLongName = longname.Replace("'", ""); mPredefinedType = type; }
		internal new static IfcDistributionSystem Parse(string strDef) { IfcDistributionSystem m = new IfcDistributionSystem(); int ipos = 0; parseFields(m, ParserSTEP.SplitLineFields(strDef), ref ipos); return m; }
		internal static void parseFields(IfcDistributionSystem c, List<string> arrFields, ref int ipos)
		{
			IfcSystem.parseFields(c, arrFields, ref ipos);
			c.mLongName = arrFields[ipos++].Replace("'", "");
			string s = arrFields[ipos++];
			if (s.StartsWith("."))
				c.mPredefinedType = (IfcDistributionSystemEnum)Enum.Parse(typeof(IfcDistributionSystemEnum), s.Replace(".", ""));
		}
		protected override string BuildString() { return mDatabase.mSchema == Schema.IFC2x3 ? "" : base.BuildString() + (mDatabase.mSchema == Schema.IFC2x3 ? "" : (mLongName == "$" ? ",$,." : ",'" + mLongName + "',.") + mPredefinedType.ToString() + "."); }
	}
	internal class IfcDocumentElectronicFormat : BaseClassIfc // DEPRECEATED IFC4
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDOCUMENTELECTRONICFORMAT";
		internal string mFileExtension = "$";//  OPTIONAL IfcLabel;
		internal string mMimeContentType = "$";//  OPTIONAL IfcLabel;
		internal string mMimeSubtype = "$";//  OPTIONAL IfcLabel;
		internal IfcDocumentElectronicFormat() : base() { }
		internal IfcDocumentElectronicFormat(IfcDocumentElectronicFormat i) : base() { mFileExtension = i.mFileExtension; mMimeContentType = i.mMimeContentType; mMimeSubtype = i.mMimeSubtype; }
		//internal IfcDocumentElectronicFormat(DatabaseIfc m, List<int> genData) : base(m, genData) { }
		internal static IfcDocumentElectronicFormat Parse(string strDef) { IfcDocumentElectronicFormat d = new IfcDocumentElectronicFormat(); int ipos = 0; parseFields(d, ParserSTEP.SplitLineFields(strDef), ref ipos); return d; }
		internal static void parseFields(IfcDocumentElectronicFormat d, List<string> arrFields, ref int ipos) { d.mFileExtension = arrFields[ipos++]; d.mMimeContentType = arrFields[ipos++]; d.mMimeSubtype = arrFields[ipos++]; }
		protected override string BuildString() { return base.BuildString() + "," + mFileExtension + "," + mMimeContentType + "," + mMimeSubtype; }
	}
	public class IfcDocumentInformation : IfcExternalInformation, IfcDocumentSelect
	{
		internal string mIdentification = "";// : IfcIdentifier;
		internal string mName;// :  IfcLabel;
		internal string mDescription = "$";// : OPTIONAL IfcText;
		internal List<int> mDocumentReferences = new List<int>(); // ifc2x3 : OPTIONAL SET [1:?] OF IfcDocumentReference;
		internal string mLocation = "$";// : IFC4	OPTIONAL IfcURIReference;
		internal string mPurpose = "$", mIntendedUse = "$", mScope = "$";// : OPTIONAL IfcText;
		internal string mRevision = "$";// : OPTIONAL IfcLabel;
		internal int mDocumentOwner;// : OPTIONAL IfcActorSelect;
		internal List<int> mEditors = new List<int>();// : OPTIONAL SET [1:?] OF IfcActorSelect;
		internal string mCreationTime = "$", mLastRevisionTime = "$";// : OPTIONAL IFC4 IfcDateTime;
		internal string mElectronicFormat = "$";// IFC4	 :	OPTIONAL IfcIdentifier; IFC4
		internal int mSSElectronicFormat;// IFC2x3 : OPTIONAL IfcDocumentElectronicFormat;
		internal string mValidFrom = "$", mValidUntil = "$";// : OPTIONAL IfcCalendarDate;
		internal IfcDocumentConfidentialityEnum mConfidentiality = IfcDocumentConfidentialityEnum.NOTDEFINED;// : OPTIONAL IfcDocumentConfidentialityEnum;
		internal IfcDocumentStatusEnum mStatus = IfcDocumentStatusEnum.NOTDEFINED;// : OPTIONAL IfcDocumentStatusEnum; 
		//INVERSE
		internal List<IfcRelAssociatesDocument> mDocumentInfoForObjects = new List<IfcRelAssociatesDocument>();//	 :	SET OF IfcRelAssociatesDocument FOR RelatingDocument;
		public List<IfcRelAssociatesDocument> Associates { get { return mDocumentInfoForObjects; } set { mDocumentInfoForObjects = value; } }
		//HasDocumentReferences	 :	SET OF IfcDocumentReference FOR ReferencedDocument;
		//IsPointedTo	 :	SET OF IfcDocumentInformationRelationship FOR RelatedDocuments;
		//IsPointer	 :	SET [0:1] OF IfcDocumentInformationRelationship FOR RelatingDocument;

		internal IfcDocumentInformation() : base() { }
		internal IfcDocumentInformation(IfcDocumentInformation i)
			: base(i)
		{
			mIdentification = i.mIdentification;
			mName = i.mName;
			mDescription = i.mDescription;
			mDocumentReferences = new List<int>(i.mDocumentReferences.ToArray());
			mPurpose = i.mPurpose;
			mIntendedUse = i.mIntendedUse;
			mScope = i.mScope;
			mRevision = i.mRevision;
			mDocumentOwner = i.mDocumentOwner;
			mEditors = new List<int>(i.mEditors.ToArray());
			mCreationTime = i.mCreationTime;
			mLastRevisionTime = i.mLastRevisionTime;
			mElectronicFormat = i.mElectronicFormat;
			mSSElectronicFormat = i.mSSElectronicFormat;
			mValidFrom = i.mValidFrom;
			mValidUntil = i.mValidUntil;
			mConfidentiality = i.mConfidentiality;
			mStatus = i.mStatus;
		}
		public IfcDocumentInformation(DatabaseIfc m, string id, string name, string desc, string location, string purpose, string use, string scope, string revision, IfcActorSelect owner,
			List<IfcActorSelect> editors, IfcDateTime creation, IfcDateTime lastRevision, string elecFormat, IfcDate validfrom, IfcDate validto, IfcDocumentConfidentialityEnum conf, IfcDocumentStatusEnum status)
			: this(m, id, name, desc, location, purpose, use, scope, revision, owner, editors, creation, lastRevision, elecFormat, validfrom, validto, conf, status, new List<int>()) { }
		internal IfcDocumentInformation(DatabaseIfc m, string id, string name, string desc, string location, string purpose, string use, string scope, string revision, IfcActorSelect owner,
			List<IfcActorSelect> editors, IfcDateTime creation, IfcDateTime lastRevision, string elecFormat, IfcDate validfrom, IfcDate validto, IfcDocumentConfidentialityEnum conf, IfcDocumentStatusEnum status, List<int> genData)
			: base(m)
		{
			mIdentification = id.Replace("'", "");
			mName = name.Replace("'", "");
			if (!string.IsNullOrEmpty(desc))
				mDescription = desc.Replace("'", "");
			if (!string.IsNullOrEmpty(location))
				mLocation = location.Replace("'", "");
			if (!string.IsNullOrEmpty(purpose))
				mPurpose = purpose.Replace("'", "");
			if (!string.IsNullOrEmpty(use))
				mIntendedUse = use.Replace("'", "");
			if (!string.IsNullOrEmpty(scope))
				mScope = scope.Replace("'", "");
			if (!string.IsNullOrEmpty(revision))
				mRevision = revision.Replace("'", "");
			if (owner != null)
				mDocumentOwner = owner.Index;
			if (editors != null && editors.Count > 0)
				mEditors.AddRange(editors.ConvertAll(x => x.Index));
			if (creation != null)
				mCreationTime = creation.ToString();
			if (lastRevision != null)
				mLastRevisionTime = lastRevision.ToString();
			if (!string.IsNullOrEmpty(elecFormat))
				mElectronicFormat = elecFormat.Replace("'", "");
			if (validfrom != null)
				mValidFrom = validfrom.ToString();
			if (validto != null)
				mValidUntil = validto.ToString();
			mConfidentiality = conf;
			mStatus = status;
			genData.Add(mIndex);
		}
		protected override string BuildString()
		{
			if (mDatabase.mSchema == Schema.IFC2x3)
			{
				return "";//to be implemented
			}
			string str = base.BuildString() + "," + mIdentification + "," + mName + "," + mDescription;
			if (mDocumentReferences.Count == 0)
				str += ",$,";
			else
			{
				str += ",(" + ParserSTEP.LinkToString(mDocumentReferences[0]);
				for (int icounter = 1; icounter < mDocumentReferences.Count; icounter++)
					str += "," + ParserSTEP.LinkToString(mDocumentReferences[0]);
				str += "),";
			}
			str += mPurpose + "," + mIntendedUse + "," + mScope + "," + mRevision + "," + ParserSTEP.LinkToString(mDocumentOwner);
			if (mEditors.Count == 0)
				str += ",$,";
			else
			{
				str += ",(" + ParserSTEP.LinkToString(mEditors[0]);
				for (int icounter = 1; icounter < mEditors.Count; icounter++)
					str += "," + ParserSTEP.LinkToString(mEditors[0]);
				str += "),";
			}
			return str + mCreationTime + "," + mLastRevisionTime + "," + (mDatabase.mSchema == Schema.IFC2x3 ? ParserSTEP.LinkToString(mSSElectronicFormat) : (mElectronicFormat == "$" ? "$" : "'" + mElectronicFormat + "'")) + "," +
				mValidFrom + "," + mValidUntil + (mConfidentiality == IfcDocumentConfidentialityEnum.NOTDEFINED ? ",$," : ",." + mConfidentiality.ToString() + ".,") + (mStatus == IfcDocumentStatusEnum.NOTDEFINED ? "$" : "." + mStatus.ToString() + ".");
		}
		internal static IfcDocumentInformation Parse(string strDef, Schema schema) { IfcDocumentInformation d = new IfcDocumentInformation(); int ipos = 0; parseFields(d, ParserSTEP.SplitLineFields(strDef), ref ipos, schema); return d; }
		internal static void parseFields(IfcDocumentInformation d, List<string> arrFields, ref int ipos, Schema schema)
		{
			IfcExternalInformation.parseFields(d, arrFields, ref ipos);
			d.mIdentification = arrFields[ipos++].Replace("'", "");
			d.mName = arrFields[ipos++].Replace("'", "");
			d.mDescription = arrFields[ipos++].Replace("'", "");
			if (schema == Schema.IFC2x3)
				d.mDocumentReferences = ParserSTEP.SplitListLinks(arrFields[ipos++]);
			else
				d.mLocation = arrFields[ipos++].Replace("'", "");
			d.mPurpose = arrFields[ipos++].Replace("'", "");
			d.mIntendedUse = arrFields[ipos++].Replace("'", "");
			d.mScope = arrFields[ipos++].Replace("'", "");
			d.mRevision = arrFields[ipos++].Replace("'", "");
			d.mDocumentOwner = ParserSTEP.ParseLink(arrFields[ipos++]);
			string s = arrFields[ipos++];
			if (s[0] != '$')
				d.mEditors = ParserSTEP.SplitListLinks(s);

			d.mCreationTime = arrFields[ipos++];
			d.mLastRevisionTime = arrFields[ipos++];
			if (schema == Schema.IFC2x3)
				d.mSSElectronicFormat = ParserSTEP.ParseLink(arrFields[ipos++]);
			else
				d.mElectronicFormat = arrFields[ipos++].Replace("'", "");
			d.mValidFrom = arrFields[ipos++];
			d.mValidUntil = arrFields[ipos++];
			s = arrFields[ipos++];
			if (s[0] == '.')
				d.mConfidentiality = (IfcDocumentConfidentialityEnum)Enum.Parse(typeof(IfcDocumentConfidentialityEnum), s.Substring(1, s.Length - 2));
			s = arrFields[ipos++];
			if (s[0] == '.')
				d.mStatus = (IfcDocumentStatusEnum)Enum.Parse(typeof(IfcDocumentStatusEnum), s.Substring(1, s.Length - 2));
		}
	}
	public class IfcDocumentInformationRelationship : BaseClassIfc
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDOCUMENTINFORMATIONRELATIONSHIP";
		internal int mRelatingDocument; //: IfcDocumentInformation
		internal List<int> mRelatedDocuments = new List<int>();// : SET [1:?] OF IfcDocumentInformation;
		internal string mRelationshipType = "$";// : OPTIONAL IfcLabel;
		internal IfcDocumentInformationRelationship() : base() { }
		internal IfcDocumentInformationRelationship(IfcDocumentInformationRelationship v) : base() { mRelatingDocument = v.mRelatingDocument; mRelatedDocuments = new List<int>(v.mRelatedDocuments.ToArray()); mRelationshipType = v.mRelationshipType; }
		internal static void parseFields(IfcDocumentInformationRelationship r, List<string> arrFields, ref int ipos) { r.mRelatingDocument = ParserSTEP.ParseLink(arrFields[ipos++]); r.mRelatedDocuments = ParserSTEP.SplitListLinks(arrFields[ipos++]); r.mRelationshipType = arrFields[ipos++]; }
		internal static IfcDocumentInformationRelationship Parse(string strDef) { IfcDocumentInformationRelationship r = new IfcDocumentInformationRelationship(); int ipos = 0; parseFields(r, ParserSTEP.SplitLineFields(strDef), ref ipos); return r; }
		protected override string BuildString()
		{
			string str = base.BuildString() + "," + ParserSTEP.LinkToString(mRelatingDocument) + ",(" + ParserSTEP.LinkToString(mRelatedDocuments[0]);
			for (int icounter = 0; icounter < mRelatedDocuments.Count; icounter++)
				str += "," + ParserSTEP.LinkToString(mRelatedDocuments[icounter]);
			return str + ")," + mRelationshipType;
		}
	}
	public partial class IfcDocumentReference : IfcExternalReference, IfcDocumentSelect
	{
		internal string mDescription = "$";// IFC4	 :	OPTIONAL IfcText;
		internal int mReferencedDocument = 0;// IFC	 :	OPTIONAL IfcDocumentInformation;
		//INVERSE
		internal List<IfcRelAssociatesDocument> mDocumentRefForObjects = new List<IfcRelAssociatesDocument>();//	 :	SET OF IfcRelAssociatesDocument FOR RelatingDocument;

		public string Description { get { return (mDescription == "$" ? "" : ParserIfc.Decode(mDescription)); } set { mDescription = (string.IsNullOrEmpty(value) ? "$" : ParserIfc.Encode(value.Replace("'", ""))); } }
		public IfcDocumentInformation ReferencedDocumen { get { return mDatabase.mIfcObjects[mReferencedDocument] as IfcDocumentInformation; } set { mReferencedDocument = (value == null ? 0 : value.mIndex); } }
		public List<IfcRelAssociatesDocument> Associates { get { return mDocumentRefForObjects; } set { mDocumentRefForObjects = value; } }

		internal IfcDocumentReference() : base() { }
		internal IfcDocumentReference(IfcDocumentReference i) : base(i) { }
		public IfcDocumentReference(DatabaseIfc db) : base(db) { }
		internal static IfcDocumentReference Parse(string strDef, Schema schema) { IfcDocumentReference r = new IfcDocumentReference(); int ipos = 0; parseFields(r, ParserSTEP.SplitLineFields(strDef), ref ipos, schema); return r; }
		internal static void parseFields(IfcDocumentReference r, List<string> arrFields, ref int ipos, Schema schema)
		{
			IfcExternalReference.parseFields(r, arrFields, ref ipos);
			if (schema != Schema.IFC2x3)
			{
				r.mDescription = arrFields[ipos++].Replace("'", "");
				r.mReferencedDocument = ParserSTEP.ParseLink(arrFields[ipos++]);
			}
		}
		protected override string BuildString() { return base.BuildString() + (mDatabase.mSchema == Schema.IFC2x3 ? "" : (mDescription == "$" ? ",$," : ",'" + mDescription + "',") + ParserSTEP.LinkToString(mReferencedDocument)); }
	}
	public interface IfcDocumentSelect : IfcInterface //IFC4 SELECT (	IfcDocumentReference, IfcDocumentInformation);
	{
		List<IfcRelAssociatesDocument> Associates { get; set; }
	}
	public partial class IfcDoor : IfcBuildingElement
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDOOR";
		internal double mOverallHeight;// : OPTIONAL IfcPositiveLengthMeasure;
		internal double mOverallWidth;// : OPTIONAL IfcPositiveLengthMeasure;
		internal IfcDoorTypeEnum mPredefinedType = IfcDoorTypeEnum.NOTDEFINED;//: OPTIONAL IfcDoorTypeEnum; //IFC4 
		internal IfcDoorTypeOperationEnum mOperationType = IfcDoorTypeOperationEnum.NOTDEFINED;// : OPTIONAL IfcDoorTypeOperationEnum; //IFC4
		internal string mUserDefinedOperationType = "$";//	 :	OPTIONAL IfcLabel;

		public IfcDoorTypeEnum PredefinedType { get { return mPredefinedType; } set { mPredefinedType = value; } }

		internal IfcDoor() : base() { }
		internal IfcDoor(IfcDoor d) : base(d) { mOverallHeight = d.mOverallHeight; mOverallWidth = d.mOverallWidth; mPredefinedType = d.mPredefinedType; mOperationType = d.mOperationType; mUserDefinedOperationType = d.mUserDefinedOperationType; }
		public IfcDoor(IfcProduct host, IfcObjectPlacement placement, IfcProductRepresentation representation) : base(host, placement, representation) { }
		internal static IfcDoor Parse(string strDef, Schema schema) { IfcDoor d = new IfcDoor(); int ipos = 0; parseFields(d, ParserSTEP.SplitLineFields(strDef), ref ipos, schema); return d; }
		internal static void parseFields(IfcDoor d, List<string> arrFields, ref int ipos, Schema schema)
		{
			IfcBuildingElement.parseFields(d, arrFields, ref ipos);
			d.mOverallHeight = ParserSTEP.ParseDouble(arrFields[ipos++]);
			d.mOverallWidth = ParserSTEP.ParseDouble(arrFields[ipos++]);
			if (schema != Schema.IFC2x3)
			{
				string str = arrFields[ipos++];
				if (str[0] == '.')
					d.mPredefinedType = (IfcDoorTypeEnum)Enum.Parse(typeof(IfcDoorTypeEnum), str.Substring(1, str.Length - 2));
				str = arrFields[ipos++];
				if (str[0] == '.')
					d.mOperationType = (IfcDoorTypeOperationEnum)Enum.Parse(typeof(IfcDoorTypeOperationEnum), str.Substring(1, str.Length - 2));
				try
				{
					d.mUserDefinedOperationType = arrFields[ipos++].Replace("'", "");
				}
				catch (Exception) { }
			}
		}
		protected override string BuildString()
		{
			return base.BuildString() + "," + ParserSTEP.DoubleOptionalToString(mOverallHeight) + "," + ParserSTEP.DoubleOptionalToString(mOverallWidth)
				+ (mDatabase.mSchema == Schema.IFC2x3 ? "" : ",." + mPredefinedType.ToString() + ".,." + mOperationType.ToString() + (mUserDefinedOperationType == "$" ? ".,$" : ".,'" + mUserDefinedOperationType + "'"));
		}
	}
	internal class IfcDoorLiningProperties : IfcPreDefinedPropertySet // IFC2x3 IfcPropertySetDefinition
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDOORLININGPROPERTIES";
		internal double mLiningDepth, mLiningThickness, mThresholdDepth, mThresholdThickness, mTransomThickness;// : OPTIONAL IfcPositiveLengthMeasure;
		internal double mTransomOffset, mLiningOffset, mThresholdOffset;// : OPTIONAL IfcLengthMeasure;
		internal double mCasingThickness, mCasingDepth;// : OPTIONAL IfcPositiveLengthMeasure;
		private int mShapeAspectStyle;// : OPTIONAL IfcShapeAspect;  // DEPRECEATED IFC4
		internal double mLiningToPanelOffsetX, mLiningToPanelOffsetY;//	 :	OPTIONAL IfcLengthMeasure;  IFC4
		internal IfcDoorLiningProperties() : base() { }
		internal IfcDoorLiningProperties(IfcDoorLiningProperties p)
			: base(p)
		{
			mLiningDepth = p.mLiningDepth;
			mLiningThickness = p.mLiningThickness;
			mThresholdDepth = p.mThresholdDepth;
			mThresholdThickness = p.mThresholdThickness;
			mTransomThickness = p.mTransomThickness;
			mTransomOffset = p.mTransomOffset;
			mLiningOffset = p.mLiningOffset;
			mThresholdOffset = p.mThresholdOffset;
			mCasingThickness = p.mCasingThickness;
			mCasingDepth = p.mCasingDepth;
			mShapeAspectStyle = p.mShapeAspectStyle;
			mLiningToPanelOffsetX = p.mLiningToPanelOffsetX;
			mLiningToPanelOffsetY = p.mLiningToPanelOffsetY;
		}
		internal IfcDoorLiningProperties(DatabaseIfc m, string name, double lnngDpth, double lnngThck, double thresDpth, double thresThck, double trnsmThck, double trnsmOff,
			double lnngOff, double thresOff, double casThck, double casDpth, double lngToPnlOffstX, double lngToPnlOffstY, List<int> genData)
			: base(m, name, genData)
		{
			mLiningDepth = lnngDpth;
			mLiningThickness = lnngThck;
			mThresholdDepth = thresDpth;
			mThresholdThickness = thresThck;
			mTransomThickness = trnsmThck;
			mTransomOffset = trnsmOff;
			mLiningOffset = lnngOff;
			mThresholdOffset = thresOff;
			mCasingThickness = casThck;
			mCasingDepth = casDpth;
			mLiningToPanelOffsetX = lngToPnlOffstX;
			mLiningToPanelOffsetY = lngToPnlOffstY;
		}
		internal static IfcDoorLiningProperties Parse(string strDef, Schema schema) { IfcDoorLiningProperties p = new IfcDoorLiningProperties(); int ipos = 0; parseFields(p, ParserSTEP.SplitLineFields(strDef), ref ipos,schema); return p; }
		internal static void parseFields(IfcDoorLiningProperties p, List<string> arrFields, ref int ipos,Schema schema)
		{
			IfcPropertySetDefinition.parseFields(p, arrFields, ref ipos);
			p.mLiningDepth = ParserSTEP.ParseDouble(arrFields[ipos++]);
			p.mLiningThickness = ParserSTEP.ParseDouble(arrFields[ipos++]);
			p.mThresholdDepth = ParserSTEP.ParseDouble(arrFields[ipos++]);
			p.mThresholdThickness = ParserSTEP.ParseDouble(arrFields[ipos++]);
			p.mTransomThickness = ParserSTEP.ParseDouble(arrFields[ipos++]);
			p.mTransomOffset = ParserSTEP.ParseDouble(arrFields[ipos++]);
			p.mLiningOffset = ParserSTEP.ParseDouble(arrFields[ipos++]);
			p.mThresholdOffset = ParserSTEP.ParseDouble(arrFields[ipos++]);
			p.mCasingThickness = ParserSTEP.ParseDouble(arrFields[ipos++]);
			p.mCasingDepth = ParserSTEP.ParseDouble(arrFields[ipos++]);
			p.mShapeAspectStyle = ParserSTEP.ParseLink(arrFields[ipos++]);
			if (schema != Schema.IFC2x3)
			{
				p.mLiningToPanelOffsetX = ParserSTEP.ParseDouble(arrFields[ipos++]);
				p.mLiningToPanelOffsetY = ParserSTEP.ParseDouble(arrFields[ipos++]);
			}
		}
		protected override string BuildString()
		{
			return base.BuildString() + "," + ParserSTEP.DoubleOptionalToString(mLiningDepth) + "," + ParserSTEP.DoubleOptionalToString(mLiningThickness) + "," + ParserSTEP.DoubleOptionalToString(mThresholdDepth) + "," + ParserSTEP.DoubleOptionalToString(mThresholdThickness) + "," + ParserSTEP.DoubleOptionalToString(mTransomThickness) + "," + ParserSTEP.DoubleOptionalToString(mTransomOffset) + "," + ParserSTEP.DoubleOptionalToString(mLiningOffset) + "," +
				ParserSTEP.DoubleOptionalToString(mThresholdOffset) + "," + ParserSTEP.DoubleOptionalToString(mCasingThickness) + "," + ParserSTEP.DoubleOptionalToString(mCasingDepth) + "," + ParserSTEP.LinkToString(mShapeAspectStyle) + (mDatabase.mSchema == Schema.IFC2x3 ? "" : "," + ParserSTEP.DoubleOptionalToString(mLiningToPanelOffsetX) + "," + ParserSTEP.DoubleOptionalToString(mLiningToPanelOffsetY));
		}
	}
	internal class IfcDoorPanelProperties : IfcPreDefinedPropertySet //IFC2x3 IfcPropertySetDefinition
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDOORPANELPROPERTIES";
		internal double mPanelDepth;// : OPTIONAL IfcPositiveLengthMeasure;
		internal IfcDoorPanelOperationEnum mOperationType;// : IfcDoorPanelOperationEnum;
		internal double mPanelWidth;// : OPTIONAL IfcNormalisedRatioMeasure;
		internal IfcDoorPanelPositionEnum mPanelPosition;// :IfcDoorPanelPositionEnume;
		private int mShapeAspectStyle;// : OPTIONAL IfcShapeAspect;  // DEPRECEATED IFC4
		internal IfcDoorPanelProperties() : base() { }
		internal IfcDoorPanelProperties(IfcDoorPanelProperties p)
			: base(p)
		{
			mPanelDepth = p.mPanelDepth;
			mOperationType = p.mOperationType;
			mPanelWidth = p.mPanelWidth;
			mPanelPosition = p.mPanelPosition;
			mShapeAspectStyle = p.mShapeAspectStyle;
		}
		internal IfcDoorPanelProperties(DatabaseIfc m, string name, double depth, IfcDoorPanelOperationEnum op, double width, IfcDoorPanelPositionEnum pos, List<int> genData)
			: base(m, name, genData) { mPanelDepth = depth; mOperationType = op; mPanelWidth = width; mPanelPosition = pos; }
		internal static IfcDoorPanelProperties Parse(string strDef) { IfcDoorPanelProperties p = new IfcDoorPanelProperties(); int ipos = 0; parseFields(p, ParserSTEP.SplitLineFields(strDef), ref ipos); return p; }
		internal static void parseFields(IfcDoorPanelProperties p, List<string> arrFields, ref int ipos)
		{
			IfcPropertySetDefinition.parseFields(p, arrFields, ref ipos);
			p.mPanelDepth = ParserSTEP.ParseDouble(arrFields[ipos++]);
			p.mOperationType = (IfcDoorPanelOperationEnum)Enum.Parse(typeof(IfcDoorPanelOperationEnum), arrFields[ipos++].Replace(".", ""));
			p.mPanelWidth = ParserSTEP.ParseDouble(arrFields[ipos++]);
			p.mPanelPosition = (IfcDoorPanelPositionEnum)Enum.Parse(typeof(IfcDoorPanelPositionEnum), arrFields[ipos++].Replace(".", ""));
			p.mShapeAspectStyle = ParserSTEP.ParseLink(arrFields[ipos++]);
		}
		protected override string BuildString() { return base.BuildString() + "," + ParserSTEP.DoubleOptionalToString(mPanelDepth) + ",." + mOperationType.ToString() + ".," + ParserSTEP.DoubleOptionalToString(mPanelWidth) + ",." + mPanelPosition.ToString() + ".," + ParserSTEP.LinkToString(mShapeAspectStyle); }
	}
	public partial class IfcDoorStandardCase : IfcDoor
	{
		public override string KeyWord { get { return (mDatabase.mSchema == Schema.IFC2x3 || mDatabase.mModelView == ModelView.Ifc4Reference ? base.KeyWord : mKW); } }
		internal new static string mKW = "IFCDOORSTANDARDCASE";
		internal IfcDoorStandardCase() : base() { }
		internal IfcDoorStandardCase(IfcDoorStandardCase o) : base(o) { }

		internal static IfcDoorStandardCase Parse(string strDef) { IfcDoorStandardCase s = new IfcDoorStandardCase(); int ipos = 0; parseFields(s, ParserSTEP.SplitLineFields(strDef), ref ipos); return s; }
		internal static void parseFields(IfcDoorStandardCase s, List<string> arrFields, ref int ipos) { IfcDoor.parseFields(s, arrFields, ref ipos); }
	}
	public partial class IfcDoorStyle : IfcTypeProduct //IFC2x3 
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDOORSTYLE";
		internal IfcDoorTypeOperationEnum mOperationType = IfcDoorTypeOperationEnum.NOTDEFINED;// : IfcDoorStyleOperationEnum;
		internal IfcDoorStyleConstructionEnum mConstructionType = IfcDoorStyleConstructionEnum.NOTDEFINED;// : IfcDoorStyleConstructionEnum; //IFC2x3
		internal bool mParameterTakesPrecedence = false;// : BOOLEAN; 
		internal bool mSizeable = false;// : BOOLEAN;  //IFC2x3
		internal IfcDoorStyle() : base() { }
		internal IfcDoorStyle(IfcDoorStyle s) : base(s) { mOperationType = s.mOperationType; mConstructionType = s.mConstructionType; mParameterTakesPrecedence = s.mParameterTakesPrecedence; mSizeable = s.mSizeable; }
		internal new static IfcDoorStyle Parse(string strDef) { IfcDoorStyle s = new IfcDoorStyle(); int ipos = 0; parseFields(s, ParserSTEP.SplitLineFields(strDef), ref ipos); return s; }
		internal static void parseFields(IfcDoorStyle s, List<string> arrFields, ref int ipos)
		{
			IfcTypeProduct.parseFields(s, arrFields, ref ipos);
			s.mOperationType = (IfcDoorTypeOperationEnum)Enum.Parse(typeof(IfcDoorTypeOperationEnum), arrFields[ipos++].Replace(".", ""));
			s.mConstructionType = (IfcDoorStyleConstructionEnum)Enum.Parse(typeof(IfcDoorStyleConstructionEnum), arrFields[ipos++].Replace(".", ""));
			s.mParameterTakesPrecedence = ParserSTEP.ParseBool(arrFields[ipos++]);
			s.mSizeable = ParserSTEP.ParseBool(arrFields[ipos++]);
		}
		protected override string BuildString() { return base.BuildString() + ",." + mOperationType.ToString() + ".,." + mConstructionType.ToString() + ".," + ParserSTEP.BoolToString(mParameterTakesPrecedence) + "," + ParserSTEP.BoolToString(mSizeable); }

	}
	public partial class IfcDoorType : IfcBuildingElementType //IFC2x3 IfcDoorStyle
	{
		public override string KeyWord { get { return (mDatabase.mSchema == Schema.IFC2x3 ? IfcDoorStyle.mKW : mKW); } }
		internal new static string mKW = "IFCDOORTYPE";
		internal IfcDoorTypeEnum mPredefinedType = IfcDoorTypeEnum.NOTDEFINED;
		internal IfcDoorTypeOperationEnum mOperationType;// : IfcDoorStyleOperationEnum; 
		internal bool mParameterTakesPrecedence = false;// : BOOLEAN;  
		internal string mUserDefinedOperationType = "$";//	 :	OPTIONAL IfcLabel;
		public IfcDoorTypeEnum PredefinedType { get { return mPredefinedType; } set { mPredefinedType = value; } }
		public string UserDefinedOperationType { get { return (mUserDefinedOperationType == "$" ? "" : ParserIfc.Decode(mUserDefinedOperationType)); } set { mUserDefinedOperationType = (string.IsNullOrEmpty(value) ? "$" : ParserIfc.Encode(value.Replace("'", ""))); } }

		internal IfcDoorType() : base() { }
		internal IfcDoorType(IfcDoorType t) : base(t) { mPredefinedType = t.mPredefinedType; mOperationType = t.mOperationType; mParameterTakesPrecedence = t.mParameterTakesPrecedence; mUserDefinedOperationType = t.mUserDefinedOperationType; }
		public IfcDoorType(DatabaseIfc m, string name, IfcDoorTypeEnum type) : this(m, name, type, IfcDoorTypeOperationEnum.NOTDEFINED, false) { }
		internal IfcDoorType(DatabaseIfc m, string name, IfcDoorTypeEnum type, IfcDoorTypeOperationEnum operation, IfcDoorLiningProperties lp, List<IfcDoorPanelProperties> pps)
			: base(m)
		{
			Name = name;
			if (lp != null) mHasPropertySets.Add(lp.mIndex);
			if (pps != null && pps.Count > 0) mHasPropertySets.AddRange(pps.ConvertAll(x => x.mIndex));
			mPredefinedType = type;
			mOperationType = operation;
			mParameterTakesPrecedence = true;
			 
		}
		internal IfcDoorType(DatabaseIfc m, string name, IfcDoorTypeEnum type, IfcDoorTypeOperationEnum operation, bool parameterTakesPrecendence)
			: base(m)
		{
			Name = name;
			mPredefinedType = type;
			mOperationType = operation;
			mParameterTakesPrecedence = parameterTakesPrecendence;
		}

		internal new static IfcDoorType Parse(string strDef, Schema schema) { IfcDoorType s = new IfcDoorType(); int ipos = 0; parseFields(s, ParserSTEP.SplitLineFields(strDef), ref ipos,schema); return s; }
		internal static void parseFields(IfcDoorType s, List<string> arrFields, ref int ipos, Schema schema)
		{
			if (schema == Schema.IFC2x3)
				IfcTypeProduct.parseFields(s, arrFields, ref ipos);
			else
			{
				IfcElementType.parseFields(s, arrFields, ref ipos);
				s.mPredefinedType = (IfcDoorTypeEnum)Enum.Parse(typeof(IfcDoorTypeEnum), arrFields[ipos++].Replace(".", ""));
			}
			s.mOperationType = (IfcDoorTypeOperationEnum)Enum.Parse(typeof(IfcDoorTypeOperationEnum), arrFields[ipos++].Replace(".", ""));
			s.mParameterTakesPrecedence = ParserSTEP.ParseBool(arrFields[ipos++]);
			s.mUserDefinedOperationType = arrFields[ipos++].Replace("'", "");
		}
		protected override string BuildString() { return base.BuildString() + (mDatabase.mSchema == Schema.IFC2x3 ? ",." : ",." + mPredefinedType.ToString() + ".,.") + mOperationType.ToString() + (mDatabase.mSchema == Schema.IFC2x3 ? ".,.NOTDEFINED" : "") + ".," + ParserSTEP.BoolToString(mParameterTakesPrecedence) + (mDatabase.mSchema == Schema.IFC2x3 ? "," + ParserSTEP.BoolToString(false) : (mUserDefinedOperationType == "$" ? ",$" : ",'" + mUserDefinedOperationType + "'")); }
	}
	public class IfcDraughtingCallout : IfcGeometricRepresentationItem // DEPRECEATED IFC4 SUPERTYPE OF (ONEOF (IfcDimensionCurveDirectedCallout ,IfcStructuredDimensionCallout))
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDRAUGHTINGCALLOUT";
		internal List<int> mContents = new List<int>(); //: SET [1:?] OF IfcDraughtingCalloutElement 
		internal IfcDraughtingCallout() : base() { }
		internal IfcDraughtingCallout(IfcDraughtingCallout el) : base(el) { mContents = new List<int>(el.mContents.ToArray()); }
		internal static IfcDraughtingCallout Parse(string strDef) { IfcDraughtingCallout c = new IfcDraughtingCallout(); int ipos = 0; parseFields(c, ParserSTEP.SplitLineFields(strDef), ref ipos); return c; }
		internal static void parseFields(IfcDraughtingCallout c, List<string> arrFields, ref int ipos) { IfcGeometricRepresentationItem.parseFields(c, arrFields, ref ipos); c.mContents = ParserSTEP.SplitListLinks(arrFields[ipos++]); }
		protected override string BuildString()
		{
			string str = ",(" + ParserSTEP.LinkToString(mContents[0]);
			for (int icounter = 1; icounter < mContents.Count; icounter++)
				str += "," + ParserSTEP.LinkToString(mContents[icounter]);
			return str + ")";
		}
	}
	public interface IfcDraughtingCalloutElement : IfcInterface { } //SELECT (IfcAnnotationCurveOccurrence ,IfcAnnotationTextOccurrence ,IfcAnnotationSymbolOccurrence);
	public class IfcDraughtingCalloutRelationship : BaseClassIfc // DEPRECEATED IFC4
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDRAUGHTINGCALLOUTRELATIONSHIP";
		internal string mName = "$";// : OPTIONAL IfcLabel;
		internal string mDescription = "$";// : OPTIONAL IfcText;
		internal int mRelatingDraughtingCallout;// : IfcDraughtingCallout;
		internal int mRelatedDraughtingCallout;// : IfcDraughtingCallout;
		internal IfcDraughtingCalloutRelationship() : base() { }
		internal IfcDraughtingCalloutRelationship(IfcDraughtingCalloutRelationship o) : base() { mName = o.mName; mDescription = o.mDescription; mRelatingDraughtingCallout = o.mRelatingDraughtingCallout; mRelatedDraughtingCallout = o.mRelatedDraughtingCallout; }
		internal static void parseFields(IfcDraughtingCalloutRelationship r, List<string> arrFields, ref int ipos) { r.mName = arrFields[ipos++]; r.mDescription = arrFields[ipos++]; r.mRelatingDraughtingCallout = ParserSTEP.ParseLink(arrFields[ipos++]); r.mRelatedDraughtingCallout = ParserSTEP.ParseLink(arrFields[ipos++]); }
		protected override string BuildString() { return base.BuildString() + "," + mName + "," + mDescription + "," + ParserSTEP.LinkToString(mRelatingDraughtingCallout) + "," + ParserSTEP.LinkToString(mRelatedDraughtingCallout); }
		internal static IfcDraughtingCalloutRelationship Parse(string strDef) { IfcDraughtingCalloutRelationship r = new IfcDraughtingCalloutRelationship(); int ipos = 0; parseFields(r, ParserSTEP.SplitLineFields(strDef), ref ipos); return r; }
	}
	public class IfcDraughtingPreDefinedColour : IfcPreDefinedColour
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDRAUGHTINGPREDEFINEDCOLOUR";
		internal IfcDraughtingPreDefinedColour() : base() { }
		internal IfcDraughtingPreDefinedColour(IfcDraughtingPreDefinedColour i) : base(i) { }
		internal static IfcDraughtingPreDefinedColour Parse(string strDef) { IfcDraughtingPreDefinedColour c = new IfcDraughtingPreDefinedColour(); int ipos = 0; parseFields(c, ParserSTEP.SplitLineFields(strDef), ref ipos); return c; }
		internal static void parseFields(IfcDraughtingPreDefinedColour c, List<string> arrFields, ref int ipos) { IfcPreDefinedColour.parseFields(c, arrFields, ref ipos); }
	}
	public class IfcDraughtingPreDefinedCurveFont : IfcPreDefinedCurveFont
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDRAUGHTINGPREDEFINEDCURVEFONT";
		internal IfcDraughtingPreDefinedCurveFont() : base() { }
		internal IfcDraughtingPreDefinedCurveFont(IfcDraughtingPreDefinedCurveFont i) : base(i) { }
		internal static IfcDraughtingPreDefinedCurveFont Parse(string strDef) { IfcDraughtingPreDefinedCurveFont f = new IfcDraughtingPreDefinedCurveFont(); int ipos = 0; parseFields(f, ParserSTEP.SplitLineFields(strDef), ref ipos); return f; }
		internal static void parseFields(IfcDraughtingPreDefinedCurveFont f, List<string> arrFields, ref int ipos) { IfcPreDefinedCurveFont.parseFields(f, arrFields, ref ipos); }
	}
	public class IfcDraughtingPreDefinedTextFont : IfcPreDefinedTextFont // DEPRECEATED IFC4
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDRAUGHTINGPREDEFINEDTEXTFONT";
		internal IfcDraughtingPreDefinedTextFont() : base() { }
		internal IfcDraughtingPreDefinedTextFont(IfcDraughtingPreDefinedTextFont el) : base(el) { }
		internal static IfcDraughtingPreDefinedTextFont Parse(string strDef) { IfcDraughtingPreDefinedTextFont f = new IfcDraughtingPreDefinedTextFont(); int ipos = 0; parseFields(f, ParserSTEP.SplitLineFields(strDef), ref ipos); return f; }
		internal static void parseFields(IfcDraughtingPreDefinedTextFont f, List<string> arrFields, ref int ipos) { IfcPreDefinedTextFont.parseFields(f, arrFields, ref ipos); }
	}
	public class IfcDuctFitting : IfcFlowFitting //IFC4
	{
		internal IfcDuctFittingTypeEnum mPredefinedType = IfcDuctFittingTypeEnum.NOTDEFINED;// OPTIONAL : IfcDuctFittingTypeEnum;
		public IfcDuctFittingTypeEnum Predefined { get { return mPredefinedType; } set { mPredefinedType = value; } }
		internal IfcDuctFitting() : base() { }
		internal IfcDuctFitting(IfcDuctFitting b) : base(b) { mPredefinedType = b.mPredefinedType; }
		internal IfcDuctFitting(IfcProduct host, IfcObjectPlacement placement, IfcProductRepresentation representation, IfcDistributionSystem system) : base(host, placement, representation, system) { }

		internal static void parseFields(IfcDuctFitting s, List<string> arrFields, ref int ipos)
		{
			IfcEnergyConversionDevice.parseFields(s, arrFields, ref ipos);
			string str = arrFields[ipos++];
			if (str[0] == '.')
				s.mPredefinedType = (IfcDuctFittingTypeEnum)Enum.Parse(typeof(IfcDuctFittingTypeEnum), str);
		}
		internal new static IfcDuctFitting Parse(string strDef) { IfcDuctFitting s = new IfcDuctFitting(); int ipos = 0; parseFields(s, ParserSTEP.SplitLineFields(strDef), ref ipos); return s; }
		protected override string BuildString()
		{
			return base.BuildString() + (mDatabase.mSchema == Schema.IFC2x3 ? "" : (mPredefinedType == IfcDuctFittingTypeEnum.NOTDEFINED ? ",$" : ",." + mPredefinedType.ToString() + "."));
		}
	}
	public class IfcDuctFittingType : IfcFlowFittingType
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDUCTFITTINGTYPE";
		internal IfcDuctFittingTypeEnum mPredefinedType = IfcDuctFittingTypeEnum.NOTDEFINED;// : IfcDuctFittingTypeEnum;
		public IfcDuctFittingTypeEnum PredefinedType { get { return mPredefinedType; } set { mPredefinedType = value; } }
		internal IfcDuctFittingType() : base() { }
		internal IfcDuctFittingType(IfcDuctFittingType t) : base(t) { mPredefinedType = t.mPredefinedType; }
		internal IfcDuctFittingType(DatabaseIfc m, string name, IfcDuctFittingTypeEnum t) : base(m) { Name = name; PredefinedType = t; }
		internal static void parseFields(IfcDuctFittingType t, List<string> arrFields, ref int ipos) { IfcFlowFittingType.parseFields(t, arrFields, ref ipos); t.mPredefinedType = (IfcDuctFittingTypeEnum)Enum.Parse(typeof(IfcDuctFittingTypeEnum), arrFields[ipos++].Replace(".", "")); }
		internal new static IfcDuctFittingType Parse(string strDef) { IfcDuctFittingType t = new IfcDuctFittingType(); int ipos = 0; parseFields(t, ParserSTEP.SplitLineFields(strDef), ref ipos); return t; }
		protected override string BuildString() { return base.BuildString() + ",." + mPredefinedType.ToString() + "."; }
	}
	public partial class IfcDuctSegment : IfcFlowSegment //IFC4
	{
		internal IfcDuctSegmentTypeEnum mPredefinedType = IfcDuctSegmentTypeEnum.NOTDEFINED;// OPTIONAL : IfcDuctSegmentTypeEnum;
		public IfcDuctSegmentTypeEnum PredefinedType { get { return mPredefinedType; } set { mPredefinedType = value; } }

		internal IfcDuctSegment() : base() { }
		internal IfcDuctSegment(IfcDuctSegment s) : base(s) { mPredefinedType = s.mPredefinedType; }
		internal IfcDuctSegment(IfcProduct host, IfcObjectPlacement placement, IfcProductRepresentation representation, IfcDistributionSystem system) : base(host, placement, representation, system) { }

		internal static void parseFields(IfcDuctSegment s, List<string> arrFields, ref int ipos)
		{
			IfcFlowSegment.parseFields(s, arrFields, ref ipos);
			string str = arrFields[ipos++];
			if (str[0] == '.')
				s.mPredefinedType = (IfcDuctSegmentTypeEnum)Enum.Parse(typeof(IfcDuctSegmentTypeEnum), str.Substring(1, str.Length - 2));
		}
		internal new static IfcDuctSegment Parse(string strDef) { IfcDuctSegment s = new IfcDuctSegment(); int ipos = 0; parseFields(s, ParserSTEP.SplitLineFields(strDef), ref ipos); return s; }
		protected override string BuildString() { return base.BuildString() + (mDatabase.mSchema == Schema.IFC2x3 ? "" : (mPredefinedType == IfcDuctSegmentTypeEnum.NOTDEFINED ? ",$" : ",." + mPredefinedType.ToString() + ".")); }

	}
	public partial class IfcDuctSegmentType : IfcFlowSegmentType
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDUCTSEGMENTTYPE";
		internal IfcDuctSegmentTypeEnum mPredefinedType = IfcDuctSegmentTypeEnum.NOTDEFINED;// : IfcDuctSegmentTypeEnum;
		public IfcDuctSegmentTypeEnum PredefinedType { get { return mPredefinedType; } set { mPredefinedType = value; } }
		internal IfcDuctSegmentType() : base() { }
		internal IfcDuctSegmentType(IfcDuctSegmentType t) : base(t) { mPredefinedType = t.mPredefinedType; }
		internal IfcDuctSegmentType(DatabaseIfc m, string name, IfcDuctSegmentTypeEnum t) : base(m) { Name = name; mPredefinedType = t; }
		internal static void parseFields(IfcDuctSegmentType t, List<string> arrFields, ref int ipos) { IfcFlowSegmentType.parseFields(t, arrFields, ref ipos); t.mPredefinedType = (IfcDuctSegmentTypeEnum)Enum.Parse(typeof(IfcDuctSegmentTypeEnum), arrFields[ipos++].Replace(".", "")); }
		internal new static IfcDuctSegmentType Parse(string strDef) { IfcDuctSegmentType t = new IfcDuctSegmentType(); int ipos = 0; parseFields(t, ParserSTEP.SplitLineFields(strDef), ref ipos); return t; }
		protected override string BuildString() { return base.BuildString() + ",." + mPredefinedType.ToString() + "."; }

	}
	public class IfcDuctSilencer : IfcFlowTreatmentDevice //IFC4  
	{
		internal IfcDuctSilencerTypeEnum mPredefinedType = IfcDuctSilencerTypeEnum.NOTDEFINED;
		public IfcDuctSilencerTypeEnum PredefinedType { get { return mPredefinedType; } set { mPredefinedType = value; } }
		internal IfcDuctSilencer(IfcDuctSilencer a) : base(a) { mPredefinedType = a.mPredefinedType; }
		internal IfcDuctSilencer() : base() { }
		internal IfcDuctSilencer(IfcProduct host, IfcObjectPlacement placement, IfcProductRepresentation representation, IfcDistributionSystem system) : base(host, placement, representation, system) { }
		internal static void parseFields(IfcDuctSilencer a, List<string> arrFields, ref int ipos)
		{
			IfcDistributionControlElement.parseFields(a, arrFields, ref ipos);
			string s = arrFields[ipos++];
			if (s.StartsWith("."))
				a.mPredefinedType = (IfcDuctSilencerTypeEnum)Enum.Parse(typeof(IfcDuctSilencerTypeEnum), s.Replace(".", ""));
		}
		internal new static IfcDuctSilencer Parse(string strDef) { IfcDuctSilencer d = new IfcDuctSilencer(); int ipos = 0; parseFields(d, ParserSTEP.SplitLineFields(strDef), ref ipos); return d; }
		protected override string BuildString()
		{
			return base.BuildString() + (mDatabase.mSchema == Schema.IFC2x3 ? "" : (mPredefinedType == IfcDuctSilencerTypeEnum.NOTDEFINED ? ",$" : ",." + mPredefinedType.ToString() + "."));
		}
	}
	public class IfcDuctSilencerType : IfcFlowTreatmentDeviceType
	{
		public override string KeyWord { get { return mKW; } }
		internal new static string mKW = "IFCDUCTSILENCERTYPE";
		internal IfcDuctSilencerTypeEnum mPredefinedType = IfcDuctSilencerTypeEnum.NOTDEFINED;// : IfcDuctSilencerTypeEnum;
		internal IfcDuctSilencerType() : base() { }
		internal IfcDuctSilencerType(IfcDuctSilencerType be) : base(be) { mPredefinedType = be.mPredefinedType; }
		internal static void parseFields(IfcDuctSilencerType t, List<string> arrFields, ref int ipos) { IfcFlowTreatmentDeviceType.parseFields(t, arrFields, ref ipos); t.mPredefinedType = (IfcDuctSilencerTypeEnum)Enum.Parse(typeof(IfcDuctSilencerTypeEnum), arrFields[ipos++].Replace(".", "")); }
		internal new static IfcDuctSilencerType Parse(string strDef) { IfcDuctSilencerType t = new IfcDuctSilencerType(); int ipos = 0; parseFields(t, ParserSTEP.SplitLineFields(strDef), ref ipos); return t; }
		protected override string BuildString() { return base.BuildString() + ",." + mPredefinedType.ToString() + "."; }
	}
	public class IfcDuration : IfcTimeOrRatioSelect
	{
		internal int mYears, mMonths, mDays, mHours, mMinutes;
		internal double mSeconds = 0;
		public int Years { get { return mYears; } set { mYears = value; } }
		public int Months { get { return mMonths; } set { mMonths = value; } }
		public int Days { get { return mDays; } set { mDays = value; } }
		public int Hours { get { return mHours; } set { mHours = value; } }
		public int Minutes { get { return mMinutes; } set { mMinutes = value; } }
		public double Seconds { get { return mSeconds; } set { mSeconds = value; } }
		public IfcDuration() { }
		public override string ToString() { return "'P" + mYears + "Y" + mMonths + "M" + mDays + "DT" + mHours + "H" + mMinutes + "M" + mSeconds.ToString(ParserSTEP.NumberFormat) + "S'"; }
		internal double ToSeconds() { return ((((((mYears * 365) + (mMonths * 30) + mDays) * 24) + mHours) * 60) + mMinutes) * 60 + mSeconds; }
		public string String { get { return getKW + "(" + ToString() + ")"; } }
		public string getKW { get { return mKW; } }
		internal static string mKW = "IFCDURATION";
	}
}
