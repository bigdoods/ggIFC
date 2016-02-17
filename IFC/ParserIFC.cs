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
using System.Collections;
using System.Text;
using System.Reflection;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;

using GeometryGym.STEP;

namespace GeometryGym.Ifc
{
	public static class ParserIfc 
	{
		public static string Encode(string str)
		{
			string result = "";
			for (int icounter = 0; icounter < str.Length; icounter++)
			{
				char c = str[icounter];
				int i = (int)c;
				if(i < 32 || i > 126)
					result +=  "\\X2\\" + string.Format("{0:x4}", i).ToUpper() + "\\X0\\";
				else
					result += c;
			}
			return result;
		}
		public static string Decode(string str) //http://www.buildingsmart-tech.org/implementation/get-started/string-encoding/string-encoding-decoding-summary
		{
			int ilast = str.Length - 4, icounter = 0;
			string result = "";
			while (icounter < ilast)
			{
				char c = str[icounter];
				if (c == '\\')
				{
					if (str[icounter + 2] == '\\')
					{
						if (str[icounter + 1] == 'S')
						{
							char o = str[icounter + 3];
							result += (char)((int)o + 128);
							icounter += 3;
						}
						else if (str[icounter + 1] == 'X' && str.Length > icounter + 4)
						{
							string s = str.Substring(icounter + 3, 2);
							c = System.Text.Encoding.ASCII.GetChars(BitConverter.GetBytes(Convert.ToInt32(s, 16)))[0];
							//result += (char)();
							result += c;
							icounter += 4;
						}
						else
							result += str[icounter];
					}
					else if (str[icounter + 3] == '\\' && str[icounter + 2] == '2' && str[icounter + 1] == 'X')
					{
						icounter += 4;
						while (str[icounter] != '\\')
						{
							string s = str.Substring(icounter, 4);
							c = System.Text.Encoding.Unicode.GetChars(BitConverter.GetBytes(Convert.ToInt32(s, 16)))[0];
							//result += (char)();
							result += c;
							icounter += 4;
						}
						icounter += 3;
					}
					else
						result += str[icounter];
				}
				else
					result += str[icounter];
				icounter++;
			}
			while (icounter < str.Length)
				result += str[icounter++];
			return result;
		}

		public static IfcLogicalEnum ParseIFCLogical(string str) 
		{
			string s = str.Trim();
			if (str == "$")
				return IfcLogicalEnum.UNKNOWN;
			Char c = char.ToUpper(s.Replace(".", "")[0]);
			if (c == 'T')
				return IfcLogicalEnum.TRUE;
			else if (c == 'F')
				return IfcLogicalEnum.FALSE;

			return IfcLogicalEnum.UNKNOWN;
		}
		public static IfcLogicalEnum StripLogical(string s, ref int pos)
		{
			IfcLogicalEnum result = IfcLogicalEnum.UNKNOWN;
			int icounter = pos, len = s.Length;
			while (char.IsWhiteSpace(s[icounter]))
			{
				icounter++;
				if (icounter == len)
					break;
			}
			if (s[icounter] == '$')
			{
				if (++icounter < len)
				{
					while (s[icounter++] != ',')
					{
						if (icounter == len)
							break;
					}
				}
				pos = icounter;
				return result;
			}
			if (s[icounter++] != '.')
				throw new Exception("Unrecognized format!");
			char c = char.ToUpper(s[icounter++]);
			if (c == 'T')
				result = IfcLogicalEnum.TRUE;
			else if (c == 'F')
				result = IfcLogicalEnum.TRUE;
			pos = icounter + 2;
			return result;
		}
		public static string LogicalToString(IfcLogicalEnum l)
		{
			if (l == IfcLogicalEnum.TRUE)
				return ".T.";
			else if (l == IfcLogicalEnum.FALSE)
				return ".F.";
			return ".U.";
		}

		internal static void GetKeyWord(string line, out int ifcID, out string keyword, out string def)
		{
			keyword = "";
			def = "";
			ifcID = 0;
			if (string.IsNullOrEmpty(line))
				return;
			string strLine = line.Trim();
			int jlast = strLine.Length, jcounter = (line[0] == '#' ? 1 : 0);
			char c;
			for (; jcounter < jlast; jcounter++)
			{
				c = strLine[jcounter];
				if (char.IsDigit(c))
					def += c;
				else
					break;
			}
			if (!string.IsNullOrEmpty(def))
				ifcID = int.Parse(def);
			c = strLine[jcounter];
			while (c == ' ')
				c = strLine[++jcounter];
			if (strLine[jcounter] == '=')
				jcounter++;
			c = strLine[jcounter];
			while (c == ' ')
				c = strLine[++jcounter];
			if (c != 'I')
				return;
			for (; jcounter < jlast; jcounter++)
			{
				c = strLine[jcounter];
				if (c == '(')
					break;
				keyword += c;
			}
			keyword = keyword.Trim();
			keyword = keyword.ToUpper();
			int len = strLine.Length;
			int ilast = 1;
			while (strLine[len - ilast] != ')')
				ilast++;
			def = strLine.Substring(jcounter + 1, len - jcounter - ilast - 1);//(strLine[len-1] == ';' ? 3 : 2));
		}
		internal static BaseClassIfc ParseLine(string line, Schema schema)
		{
			string kw = "", str = "";
			int ifcID = 0;
			if (string.IsNullOrEmpty(line))
				return null;
			if (line.Length < 5 || line.StartsWith("ISO"))
				return null;
			GetKeyWord(line, out ifcID, out kw, out str);
			if (string.IsNullOrEmpty(kw))
				return null;
			str = str.Trim();
			BaseClassIfc result = LineParser(kw, str, schema);
			if (result == null)
				return null;
			result.mIFCString = str;
			result.mIndex = ifcID;
			return result;
		}
		private static BaseClassIfc LineParser(string keyword, string str, Schema schema)
		{
			string suffix = keyword.Substring(3);
			if (string.IsNullOrEmpty(suffix))
				return null;
			#region A to B
			if (suffix[0] == 'A')
			{
				if (string.Compare(suffix, "ACTIONREQUEST", true) == 0)
					return IfcActionRequest.Parse(str, schema);
				if (string.Compare(suffix, "ACTOR", true) == 0)
					return IfcActor.Parse(str);
				if (string.Compare(suffix, "ACTORROLE", true) == 0)
					return IfcActorRole.Parse(str);
				if (string.Compare(suffix, "ACTUATOR", true) == 0)
					return IfcActuator.Parse(str);
				if (string.Compare(suffix, "ACTUATORTYPE", true) == 0)
					return IfcActuatorType.Parse(str);
				if (string.Compare(suffix, "ADVANCEDBREP", true) == 0)
					return IfcAdvancedBrep.Parse(str);
				if (string.Compare(suffix, "ADVANCEDBREPWITHVOIDS", true) == 0)
					return IfcAdvancedBrepWithVoids.Parse(str);
				if (string.Compare(suffix, "ADVANCEDFACE", true) == 0)
					return IfcAdvancedFace.Parse(str);
				if (string.Compare(suffix, "AIRTERMINAL", true) == 0)
					return IfcAirTerminal.Parse(str);
				if (string.Compare(suffix, "AIRTERMINALBOX", true) == 0)
					return IfcAirTerminalBox.Parse(str);
				if (string.Compare(suffix, "AIRTERMINALBOXTYPE", true) == 0)
					return IfcAirTerminalBoxType.Parse(str);
				if (string.Compare(suffix, "IFCAIRTERMINALTYPE", true) == 0)
					return IfcAirTerminalType.Parse(str);
				if (string.Compare(suffix, "IFCAIRTOAIRHEATRECOVERY", true) == 0)
					return IfcAirToAirHeatRecovery.Parse(str);
				if (string.Compare(suffix, "IFCAIRTOAIRHEATRECOVERYTYPE", true) == 0)
					return IfcAirToAirHeatRecoveryType.Parse(str);
				if (string.Compare(suffix, "ALARM", true) == 0)
					return IfcAlarm.Parse(str);
				if (string.Compare(suffix, "ALARMTYPE", true) == 0)
					return IfcAlarmType.Parse(str);
				if (string.Compare(suffix, "ALIGNMENT", true) == 0)
					return IfcAlignment.Parse(str);
				if (string.Compare(suffix, "ALIGNMENT2DHORIZONTAL", true) == 0)
					return IfcAlignment2DHorizontal.Parse(str);
				if (string.Compare(suffix, "ALIGNMENT2DHORIZONTALSEGMENT", true) == 0)
					return IfcAlignment2DHorizontalSegment.Parse(str);
				if (string.Compare(suffix, "ALIGNMENT2DVERSEGCIRCULARARC", true) == 0)
					return IfcAlignment2DVerSegCircularArc.Parse(str);
				if (string.Compare(suffix, "ALIGNMENT2DVERSEGLINE", true) == 0)
					return IfcAlignment2DVerSegLine.Parse(str);
				if (string.Compare(suffix, "ALIGNMENT2DVERSEGPARABOLICARC", true) == 0)
					return IfcAlignment2DVerSegParabolicArc.Parse(str);
				if (string.Compare(suffix, "ALIGNMENT2DVERTICAL", true) == 0)
					return IfcAlignment2DVertical.Parse(str);
				if (string.Compare(suffix, "ANGULARDIMENSION", true) == 0)
					return IfcAngularDimension.Parse(str);
				if (string.Compare(keyword, IfcAnnotation.mKW, true) == 0)
					return IfcAnnotation.Parse(str);
				if (string.Compare(keyword, IfcAnnotationFillArea.mKW, true) == 0)
					return IfcAnnotationFillArea.Parse(str);
				if (string.Compare(keyword, IfcAnnotationTextOccurrence.mKW, true) == 0)
					return IfcAnnotationTextOccurrence.Parse(str);
				if (string.Compare(keyword, IfcApplication.mKW, true) == 0)
					return IfcApplication.Parse(str);
				if (string.Compare(keyword, "APPLIEDVALUE", true) == 0)
					return IfcAppliedValue.Parse(str, schema);
				if (string.Compare(keyword, IfcArbitraryClosedProfileDef.mKW, true) == 0)
					return IfcArbitraryClosedProfileDef.Parse(str);
				if (string.Compare(keyword, IfcArbitraryOpenProfileDef.mKW, true) == 0)
					return IfcArbitraryOpenProfileDef.Parse(str);
				if (string.Compare(keyword, IfcArbitraryProfileDefWithVoids.mKW, true) == 0)
					return IfcArbitraryProfileDefWithVoids.Parse(str);
				if (string.Compare(keyword, IfcAsymmetricIShapeProfileDef.mKW, true) == 0)
					return IfcAsymmetricIShapeProfileDef.Parse(str);
				if (string.Compare(suffix, "AUDIOVISUALAPPLIANCETYPE", true) == 0)
					return IfcAudioVisualAppliance.Parse(str);
				if (string.Compare(keyword, IfcAudioVisualApplianceType.mKW, true) == 0)
					return IfcAudioVisualApplianceType.Parse(str);
				if (string.Compare(keyword, IfcAxis1Placement.mKW, true) == 0)
					return IfcAxis1Placement.Parse(str);
				if (string.Compare(suffix, "AXIS2PLACEMENT2D", true) == 0)
					return IfcAxis2Placement2D.Parse(str);
				if (string.Compare(keyword, IfcAxis2Placement3D.mKW, true) == 0)
					return IfcAxis2Placement3D.Parse(str);
			}
			if (keyword[3] == 'B')
			{
				if (string.Compare(suffix, "BEAM", true) == 0)
					return IfcBeam.Parse(str, schema);
				if (string.Compare(suffix, "BEAMSTANDARDCASE", true) == 0)
					return IfcBeamStandardCase.Parse(str,schema);
				if (string.Compare(suffix, "BEAMTYPE", true) == 0)
					return IfcBeamType.Parse(str);
				if (string.Compare(keyword, IfcBezierCurve.mKW, true) == 0)
					return IfcBezierCurve.Parse(str);
				if (string.Compare(keyword, IfcBlock.mKW, true) == 0)
					return IfcBlock.Parse(str);
				if (string.Compare(suffix, "BOILER", true) == 0)
					return IfcBoiler.Parse(str);
				if (string.Compare(keyword, IfcBoilerType.mKW, true) == 0)
					return IfcBoilerType.Parse(str);
				if (string.Compare(keyword, IfcBooleanClippingResult.mKW, true) == 0)
					return IfcBooleanClippingResult.Parse(str);
				if (string.Compare(keyword, IfcBooleanResult.mKW, true) == 0)
					return IfcBooleanResult.Parse(str);
				if (string.Compare(keyword, IfcBoundaryEdgeCondition.mKW, true) == 0)
					return IfcBoundaryEdgeCondition.Parse(str);
				if (string.Compare(keyword, IfcBoundaryFaceCondition.mKW, true) == 0)
					return IfcBoundaryFaceCondition.Parse(str);
				if (string.Compare(keyword, IfcBoundaryNodeCondition.mKW, true) == 0)
					return IfcBoundaryNodeCondition.Parse(str);
				if (string.Compare(keyword, IfcBoundaryNodeConditionWarping.mKW, true) == 0)
					return IfcBoundaryNodeConditionWarping.Parse(str);
				if (string.Compare(keyword, IfcBoundingBox.mKW, true) == 0)
					return IfcBoundingBox.Parse(str);
				if (string.Compare(keyword, IfcBoxedHalfSpace.mKW, true) == 0)
					return IfcBoxedHalfSpace.Parse(str);
				if (string.Compare(keyword, IfcBSplineCurveWithKnots.mKW, true) == 0)
					return IfcBSplineCurveWithKnots.Parse(str);
				if (string.Compare(suffix, "BURNER", true) == 0)
					return IfcBurner.Parse(str);
				if (string.Compare(keyword, IfcBurnerType.mKW, true) == 0)
					return IfcBurnerType.Parse(str);
				if (string.Compare(keyword, IfcBSplineSurfaceWithKnots.mKW, true) == 0)
					return IfcBSplineSurfaceWithKnots.Parse(str);
				if (string.Compare(suffix, "BUILDING", true) == 0)
					return IfcBuilding.Parse(str);
				if (string.Compare(keyword, IfcBuildingElementPart.mKW, true) == 0)
					return IfcBuildingElementPart.Parse(str, schema);
				if (string.Compare(keyword, IfcBuildingElementPartType.mKW, true) == 0)
					return IfcBuildingElementPartType.Parse(str);
				if (string.Compare(suffix, "BUILDINGELEMENTPROXY", true) == 0)
					return IfcBuildingElementProxy.Parse(str);
				if (string.Compare(keyword, IfcBuildingElementProxyType.mKW, true) == 0)
					return IfcBuildingElementProxyType.Parse(str);
				if (string.Compare(suffix, "BUILDINGSTOREY", true) == 0)
					return IfcBuildingStorey.Parse(str);
				if (string.Compare(suffix, "BUILDINGSYSTEM", true) == 0)
					return IfcBuildingSystem.Parse(str);
				if (string.Compare(suffix, "BURNER", true) == 0)
					return IfcBurner.Parse(str);
				if (string.Compare(keyword, IfcBurnerType.mKW, true) == 0)
					return IfcBurnerType.Parse(str);
			}
			#endregion
			#region C
			if (keyword[3] == 'C')
			{
				if (string.Compare(suffix, "CARTESIANPOINT", true) == 0)
					return IfcCartesianPoint.Parse(str);

				if (string.Compare(suffix, "CABLECARRIERFITTING", true) == 0)
					return IfcCableCarrierFitting.Parse(str);
				if (string.Compare(suffix, "CABLECARRIERFITTINGTYPE", true) == 0)
					return IfcCableCarrierFittingType.Parse(str);
				if (string.Compare(suffix, "CABLECARRIERSEGMENT", true) == 0)
					return IfcCableCarrierSegment.Parse(str);
				if (string.Compare(suffix, "CABLECARRIERSEGMENTTYPE", true) == 0)
					return IfcCableCarrierSegmentType.Parse(str);
				if (string.Compare(suffix, "CABLEFITTING", true) == 0)
					return IfcCableFitting.Parse(str);
				if (string.Compare(suffix, "CABLEFITTINGTYPE", true) == 0)
					return IfcCableFittingType.Parse(str);
				if (string.Compare(suffix, "CABLESEGMENT", true) == 0)
					return IfcCableSegment.Parse(str);
				if (string.Compare(suffix, "CABLESEGMENTTYPE", true) == 0)
					return IfcCableSegmentType.Parse(str);
				if (string.Compare(suffix, "CALENDARDATE", true) == 0)
					return IfcCalendarDate.Parse(str);
				if (string.Compare(suffix, "CARTESIANPOINTLIST2D", true) == 0)
					return IfcCartesianPointList2D.Parse(str);
				if (string.Compare(suffix, "CARTESIANPOINTLIST3D", true) == 0)
					return IfcCartesianPointList3D.Parse(str);
				if (string.Compare(suffix, "CARTESIANTRANSFORMATIONOPERATOR2D", true) == 0)
					return IfcCartesianTransformationOperator2D.Parse(str);
				if (string.Compare(suffix, "CARTESIANTRANSFORMATIONOPERATOR2DNONUNIFORM", true) == 0)
					return IfcCartesianTransformationOperator2DnonUniform.Parse(str);
				if (string.Compare(suffix, "CARTESIANTRANSFORMATIONOPERATOR3D", true) == 0)
					return IfcCartesianTransformationOperator3D.Parse(str);
				if (string.Compare(suffix, "CARTESIANTRANSFORMATIONOPERATOR3DNONUNIFORM", true) == 0)
					return IfcCartesianTransformationOperator3DnonUniform.Parse(str);
				if (string.Compare(suffix, "CENTERLINEPROFILEDEF", true) == 0)
					return IfcCenterLineProfileDef.Parse(str);
				if (string.Compare(suffix, "CHAMFEREDGEFEATURE", true) == 0)
					return IfcChamferEdgeFeature.Parse(str);
				if (string.Compare(suffix, "CHILLER", true) == 0)
					return IfcChiller.Parse(str);
				if (string.Compare(suffix, "CHILLERTYPE", true) == 0)
					return IfcChillerType.Parse(str);
				if (string.Compare(suffix, "CHIMNEY", true) == 0)
					return IfcChimney.Parse(str, schema);
				if (string.Compare(suffix, "CHIMNEYTYPE", true) == 0)
					return IfcChimneyType.Parse(str);
				if (string.Compare(suffix, "CIRCLE", true) == 0)
					return IfcCircle.Parse(str);
				if (string.Compare(suffix, "CIRCLEHOLLOWPROFILEDEF", true) == 0)
					return IfcCircleHollowProfileDef.Parse(str);
				if (string.Compare(suffix, "CIRCLEPROFILEDEF", true) == 0)
					return IfcCircleProfileDef.Parse(str);
				if (string.Compare(suffix, "CIRCULARARCSEGMENT2D", true) == 0)
					return IfcCircularArcSegment2D.Parse(str);
				if (string.Compare(suffix, "CIVILELEMENT", true) == 0)
					return IfcCivilElement.Parse(str);
				if (string.Compare(suffix, "CIVILELEMENTTYPE", true) == 0)
					return IfcCivilElementType.Parse(str);
				if (string.Compare(suffix, "CLASSIFICATION", true) == 0)
					return IfcClassification.Parse(str, schema);
				if (string.Compare(suffix, "CLASSIFICATIONITEM", true) == 0)
					return IfcClassificationItem.Parse(str);
				if (string.Compare(suffix, "CLASSIFICATIONITEMRELATIONSHIP", true) == 0)
					return IfcClassificationItemRelationship.Parse(str);
				if (string.Compare(suffix, "CLASSIFICATIONNOTATION", true) == 0)
					return IfcClassificationNotation.Parse(str);
				if (string.Compare(suffix, "CLASSIFICATIONNOTATIONFACET", true) == 0)
					return IfcClassificationNotationFacet.Parse(str);
				if (string.Compare(suffix, "CLASSIFICATIONREFERENCE", true) == 0)
					return IfcClassificationReference.Parse(str,schema);
				if (string.Compare(suffix, "CLOSEDSHELL", true) == 0)
					return IfcClosedShell.Parse(str);
				if (string.Compare(suffix, "CLOTHOIDALARCSEGMENT2D", true) == 0)
					return IfcClothoidalArcSegment2D.Parse(str);
				if (string.Compare(suffix, "COIL", true) == 0)
					return IfcCoil.Parse(str);
				if (string.Compare(suffix, "COILTYPE", true) == 0)
					return IfcCoilType.Parse(str);
				if (string.Compare(keyword, IfcColourRgb.mKW, true) == 0)
					return IfcColourRgb.Parse(str);
				if (string.Compare(keyword, IfcColourRgbList.mKW, true) == 0)
					return IfcColourRgbList.Parse(str);
				if (string.Compare(suffix, "COLUMN", true) == 0)
					return IfcColumn.Parse(str,schema);
				if (string.Compare(suffix, "COLUMNSTANDARDCASE", true) == 0)
					return IfcColumnStandardCase.Parse(str,schema);
				if (string.Compare(suffix, "COLUMNTYPE", true) == 0)
					return IfcColumnType.Parse(str);
				if (string.Compare(keyword, IfcComplexProperty.mKW, true) == 0)
					return IfcComplexProperty.Parse(str);
				if (string.Compare(suffix, "COMPOSITECURVE", true) == 0)
					return IfcCompositeCurve.Parse(str);
				if (string.Compare(keyword, IfcCompositeCurveSegment.mKW, true) == 0)
					return IfcCompositeCurveSegment.Parse(str);
				if (string.Compare(keyword, IfcCompositeProfileDef.mKW, true) == 0)
					return IfcCompositeProfileDef.Parse(str);
				if (string.Compare(suffix, "COMPRESSOR", true) == 0)
					return IfcCompressor.Parse(str);
				if (string.Compare(suffix, "COMMUNICATIONSAPPLIANCE", true) == 0)
					return IfcCommunicationsAppliance.Parse(str);
				if (string.Compare(keyword, IfcCompressorType.mKW, true) == 0)
					return IfcCompressorType.Parse(str);
				if (string.Compare(suffix, "CONDENSER", true) == 0)
					return IfcCondenser.Parse(str);
				if (string.Compare(keyword, IfcCondenserType.mKW, true) == 0)
					return IfcCondenserType.Parse(str);
				if (string.Compare(keyword, IfcCondition.mKW, true) == 0)
					return IfcCondition.Parse(str);
				if (string.Compare(suffix, "CONNECTEDFACESET", true) == 0)
					return IfcConnectedFaceSet.Parse(str);
				if (string.Compare(keyword, IfcConnectionPointEccentricity.mKW, true) == 0)
					return IfcConnectionPointEccentricity.Parse(str);
				if (string.Compare(keyword, IfcConnectionCurveGeometry.mKW, true) == 0)
					return IfcConnectionCurveGeometry.Parse(str);
				if (string.Compare(keyword, IfcConnectionPointGeometry.mKW, true) == 0)
					return IfcConnectionPointGeometry.Parse(str);
				if (string.Compare(keyword, IfcConnectionSurfaceGeometry.mKW, true) == 0)
					return IfcConnectionSurfaceGeometry.Parse(str);
				if (string.Compare(suffix, "CONTROLLER", true) == 0)
					return IfcController.Parse(str);
				if (string.Compare(keyword, IfcControllerType.mKW, true) == 0)
					return IfcCoveringType.Parse(str);
				if (string.Compare(suffix, "CONVERSIONBASEDUNIT", true) == 0)
					return IfcConversionBasedUnit.Parse(str);
				if (string.Compare(suffix, "CONVERSIONBASEDUNITWITHOFFSET", true) == 0)
					return IfcConversionBasedUnitWithOffset.Parse(str);
				if (string.Compare(suffix, "COOLEDBEAM", true) == 0)
					return IfcCooledBeam.Parse(str);
				if (string.Compare(suffix, "COOLINGTOWER", true) == 0)
					return IfcCooledBeamType.Parse(str);
				if (string.Compare(keyword, IfcCoolingTowerType.mKW, true) == 0)
					return IfcCoolingTowerType.Parse(str);
				if (string.Compare(keyword, IfcCostItem.mKW, true) == 0)
					return IfcCostItem.Parse(str, schema);
				if (string.Compare(keyword, IfcCostValue.mKW, true) == 0)
					return IfcCostValue.Parse(str, schema);
				if (string.Compare(suffix, "COVERING", true) == 0)
					return IfcCovering.Parse(str);
				if (string.Compare(suffix, "COVERINGTYPE", true) == 0)
					return IfcCoveringType.Parse(str);
				if (string.Compare(keyword, IfcCraneRailAShapeProfileDef.mKW, true) == 0)
					return IfcCraneRailAShapeProfileDef.Parse(str);
				if (string.Compare(keyword, IfcCraneRailFShapeProfileDef.mKW, true) == 0)
					return IfcCraneRailFShapeProfileDef.Parse(str);
				if (string.Compare(keyword, IfcCsgSolid.mKW, true) == 0)
					return IfcCsgSolid.Parse(str);
				if (string.Compare(keyword, IfcCShapeProfileDef.mKW, true) == 0)
					return IfcCShapeProfileDef.Parse(str);
				if (string.Compare(keyword, IfcCurtainWall.mKW, true) == 0)
					return IfcCurtainWall.Parse(str, schema);
				if (string.Compare(keyword, IfcCurtainWallType.mKW, true) == 0)
					return IfcCurtainWallType.Parse(str);
				if (string.Compare(suffix, "CURVEBOUNDEDPLANE", true) == 0)
					return IfcCurveBoundedPlane.Parse(str);
				if (string.Compare(suffix, "CURVEBOUNDEDSURFACE", true) == 0)
					return IfcCurveBoundedSurface.Parse(str);
				if (string.Compare(keyword, IfcCurveStyle.mKW, true) == 0)
					return IfcCurveStyle.Parse(str, schema);
				if (string.Compare(keyword, IfcCurveStyleFont.mKW, true) == 0)
					return IfcCurveStyleFont.Parse(str);
				if (string.Compare(keyword, IfcCurveStyleFontAndScaling.mKW, true) == 0)
					return IfcCurveStyleFontAndScaling.Parse(str);
				if (string.Compare(keyword, IfcCurveStyleFontPattern.mKW, true) == 0)
					return IfcCurveStyleFontPattern.Parse(str);
			}
			#endregion
			#region D
			if (keyword[3] == 'D')
			{
				if (string.Compare(suffix, "DIRECTION", true) == 0)
					return IfcDirection.Parse(str);

				if (string.Compare(suffix, "DAMPER", true) == 0)
					return IfcDamper.Parse(str);
				if (string.Compare(keyword, IfcDamperType.mKW, true) == 0)
					return IfcDamperType.Parse(str);
				if (string.Compare(keyword, IfcDateAndTime.mKW, true) == 0)
					return IfcDateAndTime.Parse(str);
				if (string.Compare(keyword, IfcDerivedProfileDef.mKW, true) == 0)
					return IfcDerivedProfileDef.Parse(str);
				if (string.Compare(suffix, "DERIVEDUNIT", true) == 0)
					return IfcDerivedUnit.Parse(str);
				if (string.Compare(suffix, "DERIVEDUNITELEMENT", true) == 0)
					return IfcDerivedUnitElement.Parse(str);
				if (string.Compare(suffix, "DIMENSIONALEXPONENTS", true) == 0)
					return IfcDimensionalExponents.Parse(str);
				if (string.Compare(keyword, IfcDiscreteAccessory.mKW, true) == 0)
					return IfcDiscreteAccessory.Parse(str, schema);
				if (string.Compare(suffix, "DISCRETEACCESSORYTYPE", true) == 0)
					return IfcDiscreteAccessoryType.Parse(str);
				if (string.Compare(suffix, "DISTRIBUTIONCHAMBERELEMENT", true) == 0)
					return IfcDistributionChamberElement.Parse(str);
				if (string.Compare(keyword, IfcDistributionChamberElementType.mKW, true) == 0)
					return IfcDistributionChamberElementType.Parse(str);
				if (string.Compare(suffix, "DISTRIBUTIONCONTROLELEMENT", true) == 0)
					return IfcDistributionControlElement.Parse(str);
				if (string.Compare(suffix, "DISTRIBUTIONCONTROLELEMENTTYPE", true) == 0)
					return IfcDistributionControlElementType.Parse(str);
				if (string.Compare(suffix, "DISTRIBUTIONELEMENT", true) == 0)
					return IfcDistributionElement.Parse(str);
				if (string.Compare(suffix, "DISTRIBUTIONELEMENTTYPE", true) == 0)
					return IfcDistributionElementType.Parse(str);
				if (string.Compare(suffix, "DISTRIBUTIONFLOWELEMENT", true) == 0)
					return IfcDistributionFlowElement.Parse(str);
				if (string.Compare(suffix, "DISTRIBUTIONPORT", true) == 0)
					return IfcDistributionPort.Parse(str, schema);
				if (string.Compare(suffix, "DISTRIBUTIONCIRCUIT", true) == 0)
					return IfcDistributionCircuit.Parse(str);
				if (string.Compare(suffix, "DISTRIBUTIONSYSTEM", true) == 0)
					return IfcDistributionSystem.Parse(str);
				if (string.Compare(suffix, "DOCUMENTINFORMATION", true) == 0)
					return IfcDocumentInformation.Parse(str, schema);
				if (string.Compare(suffix, "DOCUMENTREFERENCE", true) == 0)
					return IfcDocumentReference.Parse(str, schema);
				if (string.Compare(keyword, IfcDoor.mKW, true) == 0)
					return IfcDoor.Parse(str, schema);
				if (string.Compare(keyword, IfcDoorStandardCase.mKW, true) == 0)
					return IfcDoorStandardCase.Parse(str);
				if (string.Compare(keyword, IfcDoorLiningProperties.mKW, true) == 0)
					return IfcDoorLiningProperties.Parse(str,schema);
				if (string.Compare(keyword, IfcDoorPanelProperties.mKW, true) == 0)
					return IfcDoorPanelProperties.Parse(str);
				if (string.Compare(keyword, IfcDoorStandardCase.mKW, true) == 0)
					return IfcDoorStandardCase.Parse(str);
				if (string.Compare(keyword, IfcDoorStyle.mKW, true) == 0)
					return IfcDoorStyle.Parse(str);
				if (string.Compare(keyword, IfcDoorType.mKW, true) == 0)
					return IfcDoorType.Parse(str);
				if (string.Compare(keyword, IfcDraughtingPreDefinedColour.mKW, true) == 0)
					return IfcDraughtingPreDefinedColour.Parse(str);
				if (string.Compare(keyword, IfcDraughtingPreDefinedCurveFont.mKW, true) == 0)
					return IfcDraughtingPreDefinedCurveFont.Parse(str);
				if (string.Compare(keyword, IfcDraughtingPreDefinedCurveFont.mKW, true) == 0)
					return IfcDraughtingPreDefinedCurveFont.Parse(str);
				if (string.Compare(suffix, "DUCTFITTING", true) == 0)
					return IfcDuctFitting.Parse(str);
				if (string.Compare(keyword, IfcDuctFittingType.mKW, true) == 0)
					return IfcDuctFittingType.Parse(str);
				if (string.Compare(suffix, "DUCTSEGMENT", true) == 0)
					return IfcDuctSegment.Parse(str);
				if (string.Compare(keyword, IfcDuctSegmentType.mKW, true) == 0)
					return IfcDuctSegmentType.Parse(str);
				if (string.Compare(suffix, "DUCTSILENCER", true) == 0)
					return IfcDuctSilencer.Parse(str);
				if (string.Compare(suffix, "DUCTSILENCERTYPE", true) == 0)
					return IfcDuctSilencerType.Parse(str);
			}
			#endregion
			#region E
			if (keyword[3] == 'E')
			{
				if (string.Compare(suffix, "EXTRUDEDAREASOLID", true) == 0)
					return IfcExtrudedAreaSolid.Parse(str);

				if (string.Compare(suffix, "EDGE", true) == 0)
					return IfcEdge.Parse(str);
				if (string.Compare(suffix, "EDGECURVE", true) == 0)
					return IfcEdgeCurve.Parse(str);
				if (string.Compare(keyword, IfcEdgeLoop.mKW, true) == 0)
					return IfcEdgeLoop.Parse(str);
				if (string.Compare(suffix, "ELECTRICAPPLIANCE", true) == 0)
					return IfcElectricAppliance.Parse(str);
				if (string.Compare(keyword, IfcElectricApplianceType.mKW, true) == 0)
					return IfcElectricApplianceType.Parse(str);
				if (string.Compare(suffix, "ELECTRICDISTRIBUTIONBOARD", true) == 0)
					return IfcElectricDistributionBoard.Parse(str);
				if (string.Compare(keyword, IfcElectricDistributionBoardType.mKW, true) == 0)
					return IfcElectricDistributionBoardType.Parse(str);
				if (string.Compare(suffix, "ELECTRICALDISTRIBUTIONPOINT", true) == 0)
					return IfcElectricDistributionPoint.Parse(str);
				if (string.Compare(suffix, "ELECTRICFLOWSTORAGEDEVICE", true) == 0)
					return IfcElectricFlowStorageDevice.Parse(str);
				if (string.Compare(keyword, IfcElectricFlowStorageDeviceType.mKW, true) == 0)
					return IfcElectricFlowStorageDeviceType.Parse(str);
				if (string.Compare(suffix, "ELECTRICGENERATOR", true) == 0)
					return IfcElectricGenerator.Parse(str);
				if (string.Compare(keyword, IfcElectricGeneratorType.mKW, true) == 0)
					return IfcElectricGeneratorType.Parse(str);
				if (string.Compare(suffix, "ELECTRICMOTOR", true) == 0)
					return IfcElectricMotor.Parse(str);
				if (string.Compare(keyword, IfcElectricMotorType.mKW, true) == 0)
					return IfcElectricMotorType.Parse(str);
				if (string.Compare(suffix, "IFCELECTRICTIMECONTROL", true) == 0)
					return IfcElectricTimeControl.Parse(str);
				if (string.Compare(keyword, IfcElectricTimeControlType.mKW, true) == 0)
					return IfcElectricTimeControlType.Parse(str);
				if (string.Compare(keyword, IfcElementQuantity.mKW, true) == 0)
					return IfcElementQuantity.Parse(str);
				if (string.Compare(suffix, "ELEMENTASSEMBLY", true) == 0)
					return IfcElementAssembly.Parse(str);
				if (string.Compare(suffix, "ELEMENTASSEMBLYTYPE", true) == 0)
					return IfcElementAssemblyType.Parse(str);
				if (string.Compare(keyword, IfcEllipse.mKW, true) == 0)
					return IfcEllipse.Parse(str);
				if (string.Compare(keyword, IfcEllipseProfileDef.mKW, true) == 0)
					return IfcEllipseProfileDef.Parse(str);
				if (string.Compare(suffix, "ENERGYCONVERSIONDEVICE", true) == 0)
					return IfcEnergyConversionDevice.Parse(str);
				if (string.Compare(suffix, "ENGINE", true) == 0)
					return IfcEngine.Parse(str);
				if (string.Compare(suffix, "ENGINETYPE", true) == 0)
					return IfcEngineType.Parse(str);
				if (string.Compare(suffix, "EVAPORATIVECOOLER", true) == 0)
					return IfcEvaporativeCooler.Parse(str);
				if (string.Compare(suffix, "EVAPORATIVECOOLERTYPE", true) == 0)
					return IfcEvaporativeCoolerType.Parse(str);
				if (string.Compare(suffix, "EVAPORATOR", true) == 0)
					return IfcEvaporator.Parse(str);
				if (string.Compare(suffix, "EVAPORATORTYPE", true) == 0)
					return IfcEvaporatorType.Parse(str);
				if (string.Compare(keyword, IfcExtendedMaterialProperties.mKW, true) == 0)
					return IfcExtendedMaterialProperties.Parse(str);
				if (string.Compare(keyword, IfcExternalReferenceRelationship.mKW, true) == 0)
					return IfcExternalReferenceRelationship.Parse(str,schema);
				if (string.Compare(suffix, "EXTERNALSPATIALELEMENT", true) == 0)
					return IfcExternalSpatialElement.Parse(str);
				if (string.Compare(suffix, "EXTRUDEDAREASOLIDTAPERED", true) == 0)
					return IfcExtrudedAreaSolidTapered.Parse(str);
			}
			#endregion
			#region F
			if (keyword[3] == 'F')
			{
				if (string.Compare(suffix, "FACE", true) == 0)
					return IfcFace.Parse(str);
				if (string.Compare(suffix, "FACEBOUND", true) == 0)
					return IfcFaceBound.Parse(str);
				if (string.Compare(suffix, "FACEBASEDSURFACEMODEL", true) == 0)
					return IfcFaceBasedSurfaceModel.Parse(str);
				if (string.Compare(suffix, "FACEOUTERBOUND", true) == 0)
					return IfcFaceOuterBound.Parse(str);
				if (string.Compare(suffix, "FACESURFACE", true) == 0)
					return IfcFaceSurface.Parse(str);
				if (string.Compare(suffix, "FACETEDBREP", true) == 0)
					return IfcFacetedBrep.Parse(str);
				if (string.Compare(suffix, "FACETEDBREPWITHVOIDS", true) == 0)
					return IfcFacetedBrepWithVoids.Parse(str);
				if (string.Compare(suffix, "FAN", true) == 0)
					return IfcFan.Parse(str);
				if (string.Compare(keyword, IfcFanType.mKW, true) == 0)
					return IfcFanType.Parse(str);
				if (string.Compare(keyword, IfcFastener.mKW, true) == 0)
					return IfcFastener.Parse(str, schema);
				if (string.Compare(keyword, IfcFastenerType.mKW, true) == 0)
					return IfcFastenerType.Parse(str);
				if (string.Compare(keyword, IfcFillAreaStyle.mKW, true) == 0)
					return IfcFillAreaStyle.Parse(str);
				if (string.Compare(keyword, IfcFillAreaStyleHatching.mKW, true) == 0)
					return IfcFillAreaStyleHatching.Parse(str);
				if (string.Compare(suffix, "FILTER", true) == 0)
					return IfcFilter.Parse(str);
				if (string.Compare(keyword, IfcFilterType.mKW, true) == 0)
					return IfcFilterType.Parse(str);
				if (string.Compare(suffix, "FIRESUPPRESSIONTERMINAL", true) == 0)
					return IfcFireSuppressionTerminal.Parse(str);
				if (string.Compare(keyword, IfcFireSuppressionTerminalType.mKW, true) == 0)
					return IfcFireSuppressionTerminalType.Parse(str);
				if (string.Compare(keyword, IfcFixedReferenceSweptAreaSolid.mKW, true) == 0)
					return IfcFixedReferenceSweptAreaSolid.Parse(str);
				if (string.Compare(suffix, "FLOWCONTROLLER", true) == 0)
					return IfcFlowController.Parse(str);
				if (string.Compare(suffix, "FLOWCONTROLLERTYPE", true) == 0)
					return IfcFlowControllerType.Parse(str);
				if (string.Compare(suffix, "FLOWFITTING", true) == 0)
					return IfcFlowFitting.Parse(str);
				if (string.Compare(suffix, "FLOWFITTINGTYPE", true) == 0)
					return IfcFlowFittingType.Parse(str);
				if (string.Compare(keyword, "FLOWINSTRUMENT", true) == 0)
					return IfcFlowInstrument.Parse(str);
				if (string.Compare(keyword, IfcFlowInstrumentType.mKW, true) == 0)
					return IfcFlowInstrumentType.Parse(str);
				if (string.Compare(suffix, "FLOWMETER", true) == 0)
					return IfcFlowMeter.Parse(str);
				if (string.Compare(keyword, IfcFlowMeterType.mKW, true) == 0)
					return IfcFlowMeterType.Parse(str);
				if (string.Compare(suffix, "FLOWMOVINGDEVICE", true) == 0)
					return IfcFlowMovingDevice.Parse(str);
				if (string.Compare(suffix, "FLOWSEGMENT", true) == 0)
					return IfcFlowSegment.Parse(str);
				if (string.Compare(suffix, "FLOWSTORAGEDEVICE", true) == 0)
					return IfcFlowStorageDevice.Parse(str);
				if (string.Compare(suffix, "FLOWTERMINAL", true) == 0)
					return IfcFlowTerminal.Parse(str);
				if (string.Compare(suffix, "FLOWTREATMENTDEVICE", true) == 0)
					return IfcFlowTreatmentDevice.Parse(str);
				if (string.Compare(keyword, IfcFooting.mKW, true) == 0)
					return IfcFooting.Parse(str);
				if (string.Compare(keyword, IfcFootingType.mKW, true) == 0)
					return IfcFootingType.Parse(str);
				if (string.Compare(keyword, IfcFurnishingElement.mKW, true) == 0)
					return IfcFurnishingElement.Parse(str);
				if (string.Compare(keyword, IfcFurnishingElementType.mKW, true) == 0)
					return IfcFurnishingElementType.Parse(str);
				if (string.Compare(keyword, IfcFurniture.mKW, true) == 0)
					return IfcFurniture.Parse(str);
				if (string.Compare(keyword, IfcFurnitureStandard.mKW, true) == 0)
					return IfcFurnitureStandard.Parse(str,schema);
				if (string.Compare(keyword, IfcFurnitureType.mKW, true) == 0)
					return IfcFurnitureType.Parse(str);
			}
			#endregion
			#region G
			if (keyword[3] == 'G')
			{
				if (string.Compare(keyword, IfcGasTerminalType.mKW, true) == 0)
					return IfcGasTerminalType.Parse(str);
				if (string.Compare(suffix, "GENERALPROFILEPROPERTIES", true) == 0)
					return IfcGeneralProfileProperties.Parse(str,schema);
				if (string.Compare(keyword, IfcGeneralMaterialProperties.mKW, true) == 0)
					return IfcGeneralMaterialProperties.Parse(str);
				if (string.Compare(keyword, IfcGeographicElement.mKW, true) == 0)
					return IfcGeographicElement.Parse(str);
				if (string.Compare(keyword, IfcGeographicElementType.mKW, true) == 0)
					return IfcGeographicElementType.Parse(str);
				if (string.Compare(suffix, "GEOMETRICREPRESENTATIONCONTEXT", true) == 0)
					return IfcGeometricRepresentationContext.Parse(str);
				if (string.Compare(keyword, IfcGeometricCurveSet.mKW, true) == 0)
					return IfcGeometricCurveSet.Parse(str);
				if (string.Compare(keyword, IfcGeometricRepresentationSubContext.mKW, true) == 0)
					return IfcGeometricRepresentationSubContext.Parse(str);
				if (string.Compare(keyword, IfcGeometricSet.mKW, true) == 0)
					return IfcGeometricSet.Parse(str);
				if (string.Compare(suffix, "GRID", true) == 0)
					return IfcGrid.Parse(str,schema);
				if (string.Compare(keyword, IfcGridAxis.mKW, true) == 0)
					return IfcGridAxis.Parse(str);
				if (string.Compare(keyword, IfcGridPlacement.mKW, true) == 0)
					return IfcGridPlacement.Parse(str);
				if (string.Compare(suffix, "GROUP", true) == 0)
					return IfcGroup.Parse(str);
			}
			#endregion
			#region H to L
			if (keyword[3] == 'H')
			{
				if (string.Compare(keyword, IfcHalfSpaceSolid.mKW, true) == 0)
					return IfcHalfSpaceSolid.Parse(str);
				if (string.Compare(suffix, "HEATEXCHANGER", true) == 0)
					return IfcHeatExchanger.Parse(str);
				if (string.Compare(keyword, IfcHeatExchangerType.mKW, true) == 0)
					return IfcHeatExchangerType.Parse(str);
				if (string.Compare(suffix, "HUMIDIFIER", true) == 0)
					return IfcHumidifier.Parse(str);
				if (string.Compare(suffix, "HUMIDIFIERTYPE", true) == 0)
					return IfcHumidifierType.Parse(str);
			}
			if (keyword[3] == 'I')
			{
				if (string.Compare(keyword, IfcIndexedColourMap.mKW, true) == 0)
					return IfcIndexedColourMap.Parse(str);
				if (string.Compare(keyword, IfcIndexedPolyCurve.mKW, true) == 0)
					return IfcIndexedPolyCurve.Parse(str);
				if (string.Compare(suffix, "INDEXEDTRIANGLETEXTUREMAP", true) == 0)
					return IfcIndexedTriangleTextureMap.Parse(str);
				if (string.Compare(suffix, "INTERCEPTOR", true) == 0)
					return IfcInterceptor.Parse(str);
				if (string.Compare(keyword, IfcImageTexture.mKW, true) == 0)
					return IfcImageTexture.Parse(str,schema);
				if (string.Compare(keyword, IfcIShapeProfileDef.mKW, true) == 0)
					return IfcIShapeProfileDef.Parse(str,schema);
				if (string.Compare(keyword, IfcInventory.mKW, true) == 0)
					return IfcInventory.Parse(str);
			}
			if (keyword[3] == 'J')
			{
				if (string.Compare(suffix, "JUNCTIONBOX", true) == 0)
					return IfcJunctionBox.Parse(str);
				if (string.Compare(keyword, IfcJunctionBoxType.mKW, true) == 0)
					return IfcJunctionBoxType.Parse(str);
			}
			if (keyword[3] == 'L')
			{
				if (string.Compare(suffix, "LOCALPLACEMENT", true) == 0)
					return IfcLocalPlacement.Parse(str);

				if (string.Compare(keyword, IfcLagTime.mKW, true) == 0)
					return IfcLagTime.Parse(str);
				if (string.Compare(suffix, "LAMP", true) == 0)
					return IfcLamp.Parse(str);
				if (string.Compare(keyword, IfcLampType.mKW, true) == 0)
					return IfcLampType.Parse(str);
				if (string.Compare(keyword, IfcLibraryInformation.mKW, true) == 0)
					return IfcLibraryInformation.Parse(str);
				if (string.Compare(suffix, "LIBRARYREFERENCE", true) == 0)
					return IfcLibraryReference.Parse(str);
				if (string.Compare(suffix, "LIGHTFIXTURE", true) == 0)
					return IfcLightFixture.Parse(str);
				if (string.Compare(suffix, "LIGHTFIXTURETYPE", true) == 0)
					return IfcLightFixtureType.Parse(str);
				if (string.Compare(keyword, IfcLine.mKW, true) == 0)
					return IfcLine.Parse(str);
				if (string.Compare(keyword, IfcLineSegment2D.mKW, true) == 0)
					return IfcLineSegment2D.Parse(str);
				if (string.Compare(keyword, IfcLocalTime.mKW, true) == 0)
					return IfcLocalTime.Parse(str);
				if (string.Compare(suffix, "LOOP", true) == 0)
					return IfcLoop.Parse(str);
				if (string.Compare(keyword, IfcLShapeProfileDef.mKW, true) == 0)
					return IfcLShapeProfileDef.Parse(str);
			}
			#endregion
			#region M
			if (keyword[3] == 'M')
			{
				if (string.Compare(keyword, IfcMapConversion.mKW, true) == 0)
					return IfcMapConversion.Parse(str);
				if (string.Compare(suffix, "MAPPEDITEM", true) == 0)
					return IfcMappedItem.Parse(str);
				if (string.Compare(suffix, "MATERIAL", true) == 0)
					return IfcMaterial.Parse(str, schema);
				if (string.Compare(keyword, IfcMaterialClassificationRelationship.mKW, true) == 0)
					return IfcMaterialClassificationRelationship.Parse(str);
				if (string.Compare(keyword, IfcMaterialConstituent.mKW, true) == 0)
					return IfcMaterialConstituent.Parse(str);
				if (string.Compare(keyword, IfcMaterialConstituentSet.mKW, true) == 0)
					return IfcMaterialConstituentSet.Parse(str);
				if (string.Compare(keyword, IfcMaterialDefinitionRepresentation.mKW, true) == 0)
					return IfcMaterialDefinitionRepresentation.Parse(str);
				if (string.Compare(suffix, "MATERIALLAYER", true) == 0)
					return IfcMaterialLayer.Parse(str, schema);
				if (string.Compare(suffix, "MATERIALLAYERSET", true) == 0)
					return IfcMaterialLayerSet.Parse(str, schema);
				if (string.Compare(keyword, IfcMaterialLayerSetUsage.mKW, true) == 0)
					return IfcMaterialLayerSetUsage.Parse(str, schema);
				if (string.Compare(suffix, "MATERIALLAYERSETWITHOFFSETS", true) == 0)
					return IfcMaterialLayerSetWithOffsets.Parse(str);
				if (string.Compare(suffix, "MATERIALLAYERWITHOFFSETS", true) == 0)
					return IfcMaterialLayerWithOffsets.Parse(str);
				if (string.Compare(keyword, IfcMaterialList.mKW, true) == 0)
					return IfcMaterialList.Parse(str);
				if (string.Compare(suffix, "MATERIALPROFILE", true) == 0)
					return IfcMaterialProfile.Parse(str);
				if (string.Compare(keyword, IfcMaterialProfileSet.mKW, true) == 0)
					return IfcMaterialProfileSet.Parse(str);
				if (string.Compare(keyword, IfcMaterialProfileSetUsage.mKW, true) == 0)
					return IfcMaterialProfileSetUsage.Parse(str);
				if (string.Compare(keyword, IfcMaterialProfileSetUsageTapering.mKW, true) == 0)
					return IfcMaterialProfileSetUsageTapering.Parse(str);
				if (string.Compare(suffix, "MATERIALPROFILEWITHOFFSETS", true) == 0)
					return IfcMaterialProfileWithOffsets.Parse(str);
				if (string.Compare(suffix, "MATERIALPROPERTIES", true) == 0)
                    return IfcMaterialProperties.Parse(str,schema);
				if (string.Compare(suffix, "MEASUREWITHUNIT", true) == 0)
					return IfcMeasureWithUnit.Parse(str);
				if (string.Compare(keyword, IfcMechanicalConcreteMaterialProperties.mKW, true) == 0)
					return IfcMechanicalConcreteMaterialProperties.Parse(str);
				if (string.Compare(keyword, IfcMechanicalFastener.mKW, true) == 0)
					return IfcMechanicalFastener.Parse(str, schema);
				if (string.Compare(keyword, IfcMechanicalFastenerType.mKW, true) == 0)
					return IfcMechanicalFastenerType.Parse(str);
				if (string.Compare(keyword, IfcMechanicalMaterialProperties.mKW, true) == 0)
					return IfcMechanicalMaterialProperties.Parse(str);
				if (string.Compare(keyword, IfcMechanicalSteelMaterialProperties.mKW, true) == 0)
					return IfcMechanicalSteelMaterialProperties.Parse(str);
				if (string.Compare(suffix, "MEDICALDEVICE", true) == 0)
					return IfcMedicalDevice.Parse(str);
				if (string.Compare(keyword, IfcMedicalDeviceType.mKW, true) == 0)
					return IfcMedicalDeviceType.Parse(str);
				if (string.Compare(keyword, IfcMember.mKW, true) == 0)
					return IfcMember.Parse(str,schema);
				if (string.Compare(keyword, IfcMemberType.mKW, true) == 0)
					return IfcMemberType.Parse(str);
				if (string.Compare(keyword, IfcMemberStandardCase.mKW, true) == 0)
					return IfcMemberStandardCase.Parse(str);
				if (string.Compare(suffix, "METRIC", true) == 0)
					return IfcMetric.Parse(str,schema);
				if (string.Compare(keyword, IfcMirroredProfileDef.mKW, true) == 0)
					return IfcMirroredProfileDef.Parse(str);
				if (string.Compare(keyword, IfcMonetaryUnit.mKW, true) == 0)
					return IfcMonetaryUnit.Parse(str, schema);
				if (string.Compare(suffix, "MOTORCONNECTION", true) == 0)
					return IfcMotorConnection.Parse(str);
				if (string.Compare(keyword, IfcMotorConnectionType.mKW, true) == 0)
					return IfcMotorConnectionType.Parse(str);
			}
			#endregion
			#region N to O
			//if (kw[3] == 'N') { }
			if (keyword[3] == 'O')
			{
				if (string.Compare(keyword, IfcObjective.mKW, true) == 0)
					return IfcObjective.Parse(str,schema);
				if (string.Compare(suffix, "OPENSHELL", true) == 0)
					return IfcOpenShell.Parse(str);
				if (string.Compare(keyword, IfcOpeningElement.mKW, true) == 0)
					return IfcOpeningElement.Parse(str, schema);
				if (string.Compare(keyword, IfcOpeningStandardCase.mKW, true) == 0)
					return IfcOpeningStandardCase.Parse(str);
				if (string.Compare(keyword, IfcOrganization.mKW, true) == 0)
					return IfcOrganization.Parse(str);
				if (string.Compare(suffix, "ORIENTEDEDGE", true) == 0)
					return IfcOrientedEdge.Parse(str);
				if (string.Compare(suffix, "OUTLET", true) == 0)
					return IfcOutlet.Parse(str);
				if (string.Compare(keyword, IfcOutletType.mKW, true) == 0)
					return IfcOutletType.Parse(str);
				if (string.Compare(suffix, "OWNERHISTORY", true) == 0)
					return IfcOwnerHistory.Parse(str);
			}
			#endregion
			#region P to Q
			if (keyword[3] == 'P')
			{
				if (string.Compare(suffix, "PROPERTYSINGLEVALUE", true) == 0)
					return IfcPropertySingleValue.Parse(str);
				if (string.Compare(keyword, IfcPolyloop.mKW, true) == 0)
					return IfcPolyloop.Parse(str);
				if (string.Compare(suffix, "PROPERTYSET", true) == 0)
					return IfcPropertySet.Parse(str);
				if (string.Compare(suffix, "PRODUCTDEFINITIONSHAPE", true) == 0)
					return IfcProductDefinitionShape.Parse(str);

				if (string.Compare(keyword, IfcPath.mKW, true) == 0)
					return IfcPath.Parse(str);
				if (string.Compare(keyword, IfcPerson.mKW, true) == 0)
					return IfcPerson.Parse(str);
				if (string.Compare(keyword, IfcPersonAndOrganization.mKW, true) == 0)
					return IfcPersonAndOrganization.Parse(str);
				if (string.Compare(keyword, IfcPile.mKW, true) == 0)
					return IfcPile.Parse(str);
				if (string.Compare(keyword, IfcPileType.mKW, true) == 0)
					return IfcPileType.Parse(str);
				if (string.Compare(suffix, "PIPEFITTING", true) == 0)
					return IfcPipeFitting.Parse(str);
				if (string.Compare(keyword, IfcPipeFittingType.mKW, true) == 0)
					return IfcPipeFittingType.Parse(str);
				if (string.Compare(suffix, "PIPESEGMENT", true) == 0)
					return IfcPipeSegment.Parse(str);
				if (string.Compare(keyword, IfcPipeSegmentType.mKW, true) == 0)
					return IfcPipeSegmentType.Parse(str);
				if (string.Compare(keyword, IfcPlanarBox.mKW, true) == 0)
					return IfcPlanarBox.Parse(str);
				if (string.Compare(keyword, IfcPlanarExtent.mKW, true) == 0)
					return IfcPlanarExtent.Parse(str);
				if (string.Compare(keyword, IfcPlane.mKW, true) == 0)
					return IfcPlane.Parse(str);
				if (string.Compare(keyword, IfcPlate.mKW, true) == 0)
					return IfcPlate.Parse(str, schema);
				if (string.Compare(keyword, IfcPlateStandardCase.mKW, true) == 0)
					return IfcPlateStandardCase.Parse(str);
				if (string.Compare(keyword, IfcPlateType.mKW, true) == 0)
					return IfcPlateType.Parse(str);
				if (string.Compare(keyword, IfcPointOnCurve.mKW, true) == 0)
					return IfcPointOnCurve.Parse(str);
				if (string.Compare(keyword, IfcPointOnSurface.mKW, true) == 0)
					return IfcPointOnSurface.Parse(str);
				if (string.Compare(keyword, IfcPolygonalBoundedHalfSpace.mKW, true) == 0)
					return IfcPolygonalBoundedHalfSpace.Parse(str);
				if (string.Compare(keyword, IfcPolyline.mKW, true) == 0)
					return IfcPolyline.Parse(str);
				if (string.Compare(suffix, "POSTALADDRESS", true) == 0)
					return IfcPostalAddress.Parse(str);
				if (string.Compare(keyword, IfcPresentationLayerAssignment.mKW, true) == 0)
					return IfcPresentationLayerAssignment.Parse(str);
				if (string.Compare(keyword, IfcPresentationLayerWithStyle.mKW, true) == 0)
					return IfcPresentationLayerWithStyle.Parse(str);
				if (string.Compare(keyword, IfcPresentationStyleAssignment.mKW, true) == 0)
					return IfcPresentationStyleAssignment.Parse(str);
				if (string.Compare(suffix, "PRODUCTREPRESENTATION", true) == 0)
					return IfcProductRepresentation.Parse(str);
				if (string.Compare(keyword, "IFCPROFILEDEF", true) == 0)
					return IfcProfileDef.Parse(str);
				if (string.Compare(suffix, "PROFILEPROPERTIES", true) == 0)
					return IfcProfileProperties.Parse(str, schema);
				if (string.Compare(suffix, "PROJECT", true) == 0)
					return IfcProject.Parse(str);
				if (string.Compare(keyword, IfcProjectedCRS.mKW, true) == 0)
					return IfcProjectedCRS.Parse(str);
				if (string.Compare(suffix, "PROJECTLIBRARY", true) == 0)
					return IfcProjectLibrary.Parse(str);
				if (string.Compare(keyword, IfcPropertyBoundedValue.mKW, true) == 0)
					return IfcPropertyBoundedValue.Parse(str, schema);
				if (string.Compare(keyword, IfcPropertyEnumeratedValue.mKW, true) == 0)
					return IfcPropertyEnumeratedValue.Parse(str);
				if (string.Compare(keyword, IfcPropertyEnumeration.mKW, true) == 0)
					return IfcPropertyEnumeration.Parse(str);
				if (string.Compare(keyword, IfcPropertyListValue.mKW, true) == 0)
					return IfcPropertyListValue.Parse(str);
				if (string.Compare(keyword, IfcPropertyReferenceValue.mKW, true) == 0)
					return IfcPropertyReferenceValue.Parse(str);
				if (string.Compare(keyword, IfcPropertyTableValue.mKW, true) == 0)
					return IfcPropertyTableValue.Parse(str);
				if (string.Compare(suffix, "PROTECTIVEDEVICE", true) == 0)
					return IfcProtectiveDevice.Parse(str);
				if (string.Compare(suffix, "PROTECTIVEDEVICETRIPPINGUNIT", true) == 0)
					return IfcProtectiveDeviceTrippingUnit.Parse(str);
				if (string.Compare(keyword, IfcProtectiveDeviceTrippingUnitType.mKW, true) == 0)
					return IfcProtectiveDeviceTrippingUnitType.Parse(str);
				if (string.Compare(keyword, IfcProtectiveDeviceType.mKW, true) == 0)
					return IfcProtectiveDeviceType.Parse(str);
				if (string.Compare(suffix, "PROXY", true) == 0)
					return IfcProxy.Parse(str);
				if (string.Compare(suffix, "PUMP", true) == 0)
					return IfcPump.Parse(str);
				if (string.Compare(keyword, IfcPumpType.mKW, true) == 0)
					return IfcPumpType.Parse(str);

				if (string.Compare(keyword, "IFCPARAMETERIZEDPROFILEDEF", true) == 0)
					return IfcProfileDef.Parse(str);
			}
			if (keyword[3] == 'Q')
			{
				if (string.Compare(keyword, IfcQuantityArea.mKW, true) == 0)
					return IfcQuantityArea.Parse(str, schema);
				if (string.Compare(keyword, IfcQuantityCount.mKW, true) == 0)
					return IfcQuantityCount.Parse(str, schema);
				if (string.Compare(keyword, IfcQuantityLength.mKW, true) == 0)
					return IfcQuantityLength.Parse(str, schema);
				if (string.Compare(keyword, IfcQuantityTime.mKW, true) == 0)
					return IfcQuantityTime.Parse(str, schema);
				if (string.Compare(keyword, IfcQuantityVolume.mKW, true) == 0)
					return IfcQuantityVolume.Parse(str, schema);
				if (string.Compare(keyword, IfcQuantityWeight.mKW, true) == 0)
					return IfcQuantityWeight.Parse(str, schema);
			}
			#endregion
			#region R
			if (keyword[3] == 'R')
			{
				if (keyword[4] == 'E' && keyword[5] == 'L')
				{
					//if (string.Compare(suffix, "RELAXATION", true) == 0)
					//	return IfcRelaxation.Parse(str);
					suffix = suffix.Substring(3);
					#region relationships
					if (string.Compare(suffix, "DEFINESBYPROPERTIES", true) == 0)
						return IfcRelDefinesByProperties.Parse(str);
					if (string.Compare(suffix, "CONTAINEDINSPATIALSTRUCTURE", true) == 0)
						return IfcRelContainedInSpatialStructure.Parse(str);

					if (string.Compare(suffix, "AGGREGATES", true) == 0)
						return IfcRelAggregates.Parse(str);
					if (string.Compare(suffix, "ASSIGNSTASKS", true) == 0)
						return IfcRelAssignsTasks.Parse(str);
					if (string.Compare(suffix, "ASSIGNSTOACTOR", true) == 0)
						return IfcRelAssignsToControl.Parse(str);
					if (string.Compare(suffix, "ASSIGNSTOCONTROL", true) == 0)
						return IfcRelAssignsToControl.Parse(str);
					if (string.Compare(suffix, "ASSIGNSTOGROUP", true) == 0)
						return IfcRelAssignsToGroup.Parse(str);
					if (string.Compare(suffix, "ASSIGNSTOGROUPBYFACTOR", true) == 0)
						return IfcRelAssignsToGroupByFactor.Parse(str);
					if (string.Compare(suffix, "ASSIGNSTOPROCESS", true) == 0)
						return IfcRelAssignsToProduct.Parse(str);
					if (string.Compare(suffix, "ASSIGNSTOPRODUCT", true) == 0)
						return IfcRelAssignsToProduct.Parse(str);
					//IfcRelAssignsToProjectOrder
					if (string.Compare(suffix, "ASSIGNSTORESOURCE", true) == 0)
						return IfcRelAssignsToResource.Parse(str);
					//IfcRelAssociatesAppliedValue
					//IfcRelAssociatesApproval
					if (string.Compare(suffix, "ASSOCIATESCLASSIFICATION", true) == 0)
						return IfcRelAssociatesClassification.Parse(str);
					if (string.Compare(suffix, "ASSOCIATESCONSTRAINT", true) == 0)
						return IfcRelAssociatesConstraint.Parse(str);
					if (string.Compare(suffix, "ASSOCIATESDOCUMENT", true) == 0)
						return IfcRelAssociatesDocument.Parse(str);
					if (string.Compare(suffix, "ASSOCIATESLIBRARY", true) == 0)
						return IfcRelAssociatesLibrary.Parse(str);
					if (string.Compare(suffix, "ASSOCIATESMATERIAL", true) == 0)
						return IfcRelAssociatesMaterial.Parse(str);
					if (string.Compare(suffix, "ASSOCIATESPROFILEPROPERTIES", true) == 0)
						return IfcRelAssociatesProfileProperties.Parse(str);
					if (string.Compare(suffix, "CONNECTSELEMENTS", true) == 0)
						return IfcRelConnectsElements.Parse(str);
					if (string.Compare(suffix, "CONNECTSPATHELEMENTS", true) == 0)
						return IfcRelConnectsPathElements.Parse(str);
					if (string.Compare(suffix, "CONNECTSPORTTOELEMENT", true) == 0)
						return IfcRelConnectsPortToElement.Parse(str);
					if (string.Compare(suffix, "CONNECTSPORTS", true) == 0)
						return IfcRelConnectsPorts.Parse(str);
					if (string.Compare(keyword, "CONNECTSSTRUCTURALACTIVITY", true) == 0)
						return IfcRelConnectsStructuralActivity.Parse(str);
					if (string.Compare(suffix, "CONNECTSSTRUCTURALELEMENT", true) == 0)
						return IfcRelConnectsStructuralElement.Parse(str);
					if (string.Compare(suffix, "CONNECTSSTRUCTURALMEMBER", true) == 0)
						return IfcRelConnectsStructuralMember.Parse(str);
					if (string.Compare(suffix, "CONNECTSWITHECCENTRICITY", true) == 0)
						return IfcRelConnectsWithEccentricity.Parse(str);
					if (string.Compare(suffix, "CONNECTSWITHREALIZINGELEMENTS", true) == 0)
						return IfcRelConnectsWithRealizingElements.Parse(str);
					if (string.Compare(suffix, "COVERSBLDGELEMENTS", true) == 0)
						return IfcRelCoversBldgElements.Parse(str);
					if (string.Compare(suffix, "COVERSSPACES", true) == 0)
						return IfcRelCoversSpaces.Parse(str);
					if (string.Compare(suffix, "DECLARES", true) == 0)
						return IfcRelDeclares.Parse(str);
					if (string.Compare(suffix, "DEFINESBYOBJECT", true) == 0)
						return IfcRelDefinesByObject.Parse(str);
					if (string.Compare(suffix, "DEFINESBYTYPE", true) == 0)
						return IfcRelDefinesByType.Parse(str);
					if (string.Compare(suffix, "FILLSELEMENT", true) == 0)
						return IfcRelFillsElement.Parse(str);
					if (string.Compare(suffix, "FLOWCONTROLELEMENTS", true) == 0)
						return IfcRelFlowControlElements.Parse(str);
					//IfcRelInteractionRequirements
					if (string.Compare(suffix, "NESTS", true) == 0)
						return IfcRelNests.Parse(str);
					//IfcRelOccupiesSpaces
					//IfcRelOverridesProperties
					//if (string.Compare(suffix, "PROJECTSELEMENTS", true) == 0)
					//	return IfcRelProjectsElement.Parse(str);
					//IfcRelReferencedInSpatialStructure
					//IfcRelSchedulesCostItems
					if (string.Compare(suffix, "SEQUENCE", true) == 0)
						return IfcRelSequence.Parse(str, schema);
					if (string.Compare(suffix, "SERVICESBUILDINGS", true) == 0)
						return IfcRelServicesBuildings.Parse(str);
					if (string.Compare(suffix, "SPACEBOUNDARY", true) == 0)
						return IfcRelSpaceBoundary.Parse(str);
					if (string.Compare(suffix, "SPACEBOUNDARY1STLEVEL", true) == 0)
						return IfcRelSpaceBoundary1stLevel.Parse(str);
					if (string.Compare(suffix, "SPACEBOUNDARY2NDLEVEL", true) == 0)
						return IfcRelSpaceBoundary2ndLevel.Parse(str);
					if (string.Compare(suffix, "VOIDSELEMENT", true) == 0)
						return IfcRelVoidsElement.Parse(str);
					#endregion
				}
				else
				{
					if (string.Compare(suffix, "RADIUSDIMENSION", true) == 0)
						return IfcRadiusDimension.Parse(str);
					if (string.Compare(suffix, "RAILING", true) == 0)
						return IfcRailing.Parse(str);
					if (string.Compare(suffix, "RAILINGTYPE", true) == 0)
						return IfcRailingType.Parse(str);
					if (string.Compare(keyword, "RAMP", true) == 0)
						return IfcRamp.Parse(str);
					if (string.Compare(suffix, "RAMPFLIGHT", true) == 0)
						return IfcRampFlight.Parse(str, schema);
					if (string.Compare(suffix, "RAMPFLIGHTTYPE", true) == 0)
						return IfcRampFlightType.Parse(str);
					if (string.Compare(suffix, "RAMPTYPE", true) == 0)
						return IfcRampType.Parse(str);
					if (string.Compare(suffix, "RATIONALBEZIERCURVE", true) == 0)
						return IfcRationalBezierCurve.Parse(str);
					if (string.Compare(suffix, "RATIONALBSPLINECURVEWITHKNOTS", true) == 0)
						return IfcRationalBSplineCurveWithKnots.Parse(str);
					if (string.Compare(suffix, "RATIONALBSPLINESURFACEWITHKNOTS", true) == 0)
						return IfcRationalBSplineSurfaceWithKnots.Parse(str);
					if (string.Compare(suffix, "RECTANGLEHOLLOWPROFILEDEF", true) == 0)
						return IfcRectangleHollowProfileDef.Parse(str);
					if (string.Compare(suffix, "RECTANGLEPROFILEDEF", true) == 0)
						return IfcRectangleProfileDef.Parse(str);
					if (string.Compare(suffix, "RECTANGULARPYRAMID", true) == 0)
						return IfcRectangularPyramid.Parse(str);
					if (string.Compare(suffix, "RECTANGULARTRIMMEDSURFACE", true) == 0)
						return IfcRectangularTrimmedSurface.Parse(str);
					if (string.Compare(suffix, "RECURRENCEPATTERN", true) == 0)
						return IfcRecurrencePattern.Parse(str);
					if (string.Compare(suffix, "REFERENCE", true) == 0)
						return IfcReference.Parse(str);
					//IfcReferencesValueDocument
					//IfcRegularTimeSeries
					if (string.Compare(suffix, "REINFORCEMENTBARPROPERTIES", true) == 0)
						return IfcReinforcementBarProperties.Parse(str);
					if (string.Compare(suffix, "REINFORCEMENTDEFINITIONPROPERTIES", true) == 0)
						return IfcReinforcementDefinitionProperties.Parse(str);
					if (string.Compare(suffix, "REINFORCINGBAR", true) == 0)
						return IfcReinforcingBar.Parse(str);
					if (string.Compare(suffix, "REINFORCINGBARTYPE", true) == 0)
						return IfcReinforcingBarType.Parse(str);
					if (string.Compare(suffix, "REINFORCINGMESH", true) == 0)
						return IfcReinforcingMesh.Parse(str,schema);
					if (string.Compare(suffix, "REPRESENTATION", true) == 0)
						return IfcRepresentation.Parse(str);
					if (string.Compare(suffix, "REPRESENTATIONMAP", true) == 0)
						return IfcRepresentationMap.Parse(str);
					if (string.Compare(suffix, "RESOURCECONSTRAINTRELATIONSHIP", true) == 0)
						return IfcResourceConstraintRelationship.Parse(str,schema);
					if (string.Compare(suffix, "RESOURCETIME", true) == 0)
						return IfcResourceTime.Parse(str);
					if (string.Compare(suffix, "REVOLVEDAREASOLID", true) == 0)
						return IfcRevolvedAreaSolid.Parse(str);
					//IfcRevolvedAreaSolidTapered
					if (string.Compare(suffix, "RIBPLATEPROFILEPROPERTIES", true) == 0)
						return IfcRibPlateProfileProperties.Parse(str, schema);
					if (string.Compare(suffix, "RIGHTCIRCULARCONE", true) == 0)
						return IfcRightCircularCone.Parse(str);
					if (string.Compare(suffix, "RIGHTCIRCULARCYLINDER", true) == 0)
						return IfcRightCircularCylinder.Parse(str);
					if (string.Compare(suffix, "ROOF", true) == 0)
						return IfcRoof.Parse(str);
					if (string.Compare(suffix, "ROOFTYPE", true) == 0)
						return IfcRoofType.Parse(str);
					//IfcRoundedEdgeFeature
					if (string.Compare(suffix, "ROUNDEDRECTANGLEPROFILEDEF", true) == 0)
						return IfcRoundedRectangleProfileDef.Parse(str);
					if (string.Compare(suffix, "RATIONALBSPLINESURFACEEWITHKNOTS", true) == 0) //Previous typo
						return IfcRationalBSplineSurfaceWithKnots.Parse(str);
				}


			}
			#endregion
			#region S
			if (keyword[3] == 'S')
			{
				if (string.Compare(keyword, IfcShapeRepresentation.mKW, true) == 0)
					return IfcShapeRepresentation.Parse(str);

				if (string.Compare(suffix, "SANITARYTERMINAL", true) == 0)
					return IfcSanitaryTerminal.Parse(str);
				if (string.Compare(keyword, IfcSanitaryTerminalType.mKW, true) == 0)
					return IfcSanitaryTerminalType.Parse(str);
				if (string.Compare(keyword, IfcScheduleTimeControl.mKW, true) == 0)
					return IfcScheduleTimeControl.Parse(str,schema);
				if (string.Compare(keyword, IfcSectionedSpine.mKW, true) == 0)
					return IfcSectionedSpine.Parse(str);
				if (string.Compare(suffix, "SENSOR", true) == 0)
					return IfcSensor.Parse(str);
				if (string.Compare(keyword, IfcSensorType.mKW, true) == 0)
					return IfcSensorType.Parse(str);
				if (string.Compare(suffix, "SHAPEASPECT", true) == 0)
					return IfcShapeAspect.Parse(str);
				if (string.Compare(keyword, IfcShadingDevice.mKW, true) == 0)
					return IfcShadingDevice.Parse(str, schema);
				if (string.Compare(keyword, IfcShadingDeviceType.mKW, true) == 0)
					return IfcShadingDeviceType.Parse(str);
				if (string.Compare(suffix, "SHAPEASPECT", true) == 0)
					return IfcShapeAspect.Parse(str);
				if (string.Compare(keyword, IfcShellBasedSurfaceModel.mKW, true) == 0)
					return IfcShellBasedSurfaceModel.Parse(str);
				if (string.Compare(keyword, IfcSite.mKW, true) == 0)
					return IfcSite.Parse(str);
				if (string.Compare(suffix, "SIUNIT", true) == 0)
					return IfcSIUnit.Parse(str);
				if (string.Compare(keyword, IfcSlab.mKW, true) == 0)
					return IfcSlab.Parse(str);
				if (string.Compare(keyword, IfcSlabStandardCase.mKW, true) == 0)
					return IfcSlabStandardCase.Parse(str);
				if (string.Compare(keyword, IfcSlabType.mKW, true) == 0)
					return IfcSlabType.Parse(str);
				if (string.Compare(suffix, "SOLARDEVICE", true) == 0)
					return IfcSolarDevice.Parse(str);
				if (string.Compare(keyword, IfcSolarDeviceType.mKW, true) == 0)
					return IfcSolarDeviceType.Parse(str);
				if (string.Compare(keyword, IfcSlabType.mKW, true) == 0)
					return IfcSlabType.Parse(str);
				if (string.Compare(suffix, "SPACE", true) == 0)
					return IfcSpace.Parse(str);
				if (string.Compare(suffix, "SPACEHEATER", true) == 0)
					return IfcSpaceHeater.Parse(str);
				if (string.Compare(keyword, IfcSpaceHeaterType.mKW, true) == 0)
					return IfcSpaceHeaterType.Parse(str);
				if (string.Compare(suffix, "SPACETYPE", true) == 0)
					return IfcSpaceType.Parse(str);
				if(string.Compare(suffix,"SPATIALZONE",true) == 0)
					return IfcSpatialZone.Parse(str);
				if (string.Compare(suffix, "SPATIALZONETYPE", true) == 0)
					return IfcSpatialZoneType.Parse(str);
				if (string.Compare(keyword, IfcSphere.mKW, true) == 0)
					return IfcSphere.Parse(str);
				if (string.Compare(suffix, "STACKTERMINAL", true) == 0)
					return IfcStackTerminal.Parse(str);
				if (string.Compare(keyword, IfcStackTerminalType.mKW, true) == 0)
					return IfcStackTerminalType.Parse(str);
				if (string.Compare(keyword, IfcStair.mKW, true) == 0)
					return IfcStair.Parse(str);
				if (string.Compare(suffix, "STAIRFLIGHT", true) == 0)
					return IfcStairFlight.Parse(str,schema);
				if (string.Compare(keyword, IfcStairFlightType.mKW, true) == 0)
					return IfcStairFlightType.Parse(str);
				if (string.Compare(keyword, IfcStairType.mKW, true) == 0)
					return IfcStairType.Parse(str);
				if (string.Compare(suffix, "STRUCTURALANALYSISMODEL", true) == 0)
					return IfcStructuralAnalysisModel.Parse(str);
				if (string.Compare(keyword, IfcStructuralCurveAction.mKW, true) == 0)
					return IfcStructuralCurveAction.Parse(str,schema);
				if (string.Compare(keyword, IfcStructuralCurveConnection.mKW, true) == 0)
					return IfcStructuralCurveConnection.Parse(str);
				if (string.Compare(suffix, "STRUCTURALCURVEMEMBER", true) == 0)
					return IfcStructuralCurveMember.Parse(str,schema);
				if (string.Compare(keyword, IfcStructuralCurveMemberVarying.mKW, true) == 0)
					return IfcStructuralCurveMemberVarying.Parse(str);
				if (string.Compare(keyword, IfcStructuralCurveReaction.mKW, true) == 0)
					return IfcStructuralCurveReaction.Parse(str);
				if (string.Compare(keyword, IfcStructuralLinearAction.mKW, true) == 0)
					return IfcStructuralLinearAction.Parse(str);
				if (string.Compare(keyword, IfcStructuralLoadCase.mKW, true) == 0)
					return IfcStructuralLoadCase.Parse(str);
				if (string.Compare(keyword, IfcStructuralLoadConfiguration.mKW, true) == 0)
					return IfcStructuralLoadConfiguration.Parse(str);
				if (string.Compare(keyword, IfcStructuralLoadGroup.mKW, true) == 0)
					return IfcStructuralLoadGroup.Parse(str);
				if (string.Compare(keyword, IfcStructuralLoadLinearForce.mKW, true) == 0)
					return IfcStructuralLoadLinearForce.Parse(str);
				if (string.Compare(keyword, IfcStructuralLoadPlanarForce.mKW, true) == 0)
					return IfcStructuralLoadPlanarForce.Parse(str);
				if (string.Compare(keyword, IfcStructuralLoadSingleDisplacement.mKW, true) == 0)
					return IfcStructuralLoadSingleDisplacement.Parse(str);
				if (string.Compare(keyword, IfcStructuralLoadSingleForce.mKW, true) == 0)
					return IfcStructuralLoadSingleForce.Parse(str);
				if (string.Compare(keyword, IfcStructuralLoadTemperature.mKW, true) == 0)
					return IfcStructuralLoadTemperature.Parse(str);
				if (string.Compare(keyword, IfcStructuralPlanarAction.mKW, true) == 0)
					return IfcStructuralPlanarAction.Parse(str);
				if (string.Compare(keyword, IfcStructuralPointAction.mKW, true) == 0)
					return IfcStructuralPointAction.Parse(str);
				if (string.Compare(keyword, IfcStructuralPointConnection.mKW, true) == 0)
					return IfcStructuralPointConnection.Parse(str);
				if (string.Compare(keyword, IfcStructuralPointReaction.mKW, true) == 0)
					return IfcStructuralPointReaction.Parse(str);
				if (string.Compare(suffix, "STRUCTURALPROFILEPROPERTIES", true) == 0)
					return IfcStructuralProfileProperties.Parse(str,schema);
				if (string.Compare(keyword, IfcStructuralResultGroup.mKW, true) == 0)
					return IfcStructuralResultGroup.Parse(str);
				if (string.Compare(keyword, IfcStructuralSteelProfileProperties.mKW, true) == 0)
					return IfcStructuralSteelProfileProperties.Parse(str);
				if (string.Compare(keyword, IfcStructuralSurfaceConnection.mKW, true) == 0)
					return IfcStructuralSurfaceConnection.Parse(str);
				if (string.Compare(keyword, IfcStructuralSurfaceMember.mKW, true) == 0)
					return IfcStructuralSurfaceMember.Parse(str);
				if (string.Compare(keyword, IfcStructuralSurfaceMemberVarying.mKW, true) == 0)
					return IfcStructuralSurfaceMemberVarying.Parse(str);
				if (string.Compare(keyword, IfcStyledItem.mKW, true) == 0)
					return IfcStyledItem.Parse(str);
				if (string.Compare(suffix, "STYLEDREPRESENTATION", true) == 0)
					return IfcStyledRepresentation.Parse(str);
				if (string.Compare(keyword, IfcSurfaceCurveSweptAreaSolid.mKW, true) == 0)
					return IfcSurfaceCurveSweptAreaSolid.Parse(str);
				if (string.Compare(keyword, IfcSurfaceOfLinearExtrusion.mKW, true) == 0)
					return IfcSurfaceOfLinearExtrusion.Parse(str);
				if (string.Compare(keyword, IfcSurfaceOfRevolution.mKW, true) == 0)
					return IfcSurfaceOfRevolution.Parse(str);
				if (string.Compare(keyword, IfcSurfaceStyle.mKW, true) == 0)
					return IfcSurfaceStyle.Parse(str);
				if (string.Compare(keyword, IfcSurfaceStyleRendering.mKW, true) == 0)
					return IfcSurfaceStyleRendering.Parse(str);
				if (string.Compare(keyword, IfcSurfaceStyleWithTextures.mKW, true) == 0)
					return IfcSurfaceStyleWithTextures.Parse(str);
				if (string.Compare(keyword, IfcSurfaceStyleShading.mKW, true) == 0)
					return IfcSurfaceStyleShading.Parse(str);
				if (string.Compare(keyword, IfcSweptDiskSolid.mKW, true) == 0)
					return IfcSweptDiskSolid.Parse(str);
				if (string.Compare(suffix, "SWITCHINGDEVICE", true) == 0)
					return IfcSwitchingDevice.Parse(str);
				if (string.Compare(keyword, IfcSwitchingDeviceType.mKW, true) == 0)
					return IfcSwitchingDeviceType.Parse(str);
				if (string.Compare(suffix, "SYSTEM", true) == 0)
					return IfcSystem.Parse(str);
				if (string.Compare(keyword, IfcSystemFurnitureElement.mKW, true) == 0)
					return IfcSystemFurnitureElement.Parse(str);
				if (string.Compare(keyword, IfcSystemFurnitureElementType.mKW, true) == 0)
					return IfcSystemFurnitureElementType.Parse(str);
			}
			#endregion
			#region T
			if (keyword[3] == 'T')
			{
				if (string.Compare(keyword, IfcTable.mKW, true) == 0)
					return IfcTable.Parse(str);
				if (string.Compare(suffix, "TABLECOLUMN", true) == 0)
					return IfcTableColumn.Parse(str);
				if (string.Compare(keyword, IfcTableRow.mKW, true) == 0)
					return IfcTableRow.Parse(str);
				if (string.Compare(suffix, "TANK", true) == 0)
					return IfcTank.Parse(str);
				if (string.Compare(keyword, IfcTankType.mKW, true) == 0)
					return IfcTankType.Parse(str);
				if (string.Compare(keyword, IfcTask.mKW, true) == 0)
					return IfcTask.Parse(str, schema);
				if (string.Compare(keyword, IfcTaskTime.mKW, true) == 0)
					return IfcTaskTime.Parse(str);
				if (string.Compare(suffix, "TASKTYPE", true) == 0)
					return IfcTaskType.Parse(str);
				if (string.Compare(keyword, IfcTelecomAddress.mKW, true) == 0)
					return IfcTelecomAddress.Parse(str,schema);
				if (string.Compare(keyword, IfcTextLiteral.mKW, true) == 0)
					return IfcTextLiteral.Parse(str);
				if (string.Compare(keyword, IfcTextLiteralWithExtent.mKW, true) == 0)
					return IfcTextLiteralWithExtent.Parse(str);
				if (string.Compare(keyword, IfcTextStyle.mKW, true) == 0)
					return IfcTextStyle.Parse(str,schema);
				if (string.Compare(keyword, IfcTextStyleTextModel.mKW, true) == 0)
					return IfcTextStyleTextModel.Parse(str);
				if (string.Compare(keyword, IfcTextStyleFontModel.mKW, true) == 0)
					return IfcTextStyleFontModel.Parse(str);
				if (string.Compare(keyword, IfcTextStyleForDefinedFont.mKW, true) == 0)
					return IfcTextStyleForDefinedFont.Parse(str);
				if (string.Compare(suffix, "TEXTUREVERTEXLIST", true) == 0)
					return IfcTextureVertexList.Parse(str);
				if (string.Compare(keyword, IfcThermalMaterialProperties.mKW, true) == 0)
					return IfcThermalMaterialProperties.Parse(str);
				if (string.Compare(keyword, IfcTimePeriod.mKW, true) == 0)
					return IfcTimePeriod.Parse(str);
				if (string.Compare(keyword, IfcTopologyRepresentation.mKW, true) == 0)
					return IfcTopologyRepresentation.Parse(str);
				if (string.Compare(suffix, "TRANSFORMER", true) == 0)
					return IfcTransformer.Parse(str);
				if (string.Compare(keyword, IfcTransformerType.mKW, true) == 0)
					return IfcTransformerType.Parse(str);
				if (string.Compare(keyword, IfcTransportElement.mKW, true) == 0)
					return IfcTransportElement.Parse(str);
				if (string.Compare(suffix, "TRANSPORTELEMENTTYPE", true) == 0)
					return IfcTransportElementType.Parse(str);
				if (string.Compare(keyword, IfcTrapeziumProfileDef.mKW, true) == 0)
					return IfcTrapeziumProfileDef.Parse(str);
				if (string.Compare(keyword, IfcTriangulatedFaceSet.mKW, true) == 0)
					return IfcTriangulatedFaceSet.Parse(str);
				if (string.Compare(suffix, "TRIMMEDCURVE", true) == 0)
					return IfcTrimmedCurve.Parse(str);
				if (string.Compare(keyword, IfcTShapeProfileDef.mKW, true) == 0)
					return IfcTShapeProfileDef.Parse(str);
				if (string.Compare(suffix, "TUBEBUNDLE", true) == 0)
					return IfcTubeBundle.Parse(str);
				if (string.Compare(keyword, IfcTubeBundleType.mKW, true) == 0)
					return IfcTubeBundleType.Parse(str);
				if (string.Compare(suffix, "TYPEOBJECT", true) == 0)
					return IfcTypeObject.Parse(str);
				if (string.Compare(suffix, "TYPEPRODUCT", true) == 0)
					return IfcTypeProduct.Parse(str);
			}
			#endregion
			#region U to Z
			else
			{
				if (string.Compare(suffix, "UNITARYCONTROLELEMENT", true) == 0)
					return IfcUnitaryControlElement.Parse(str);
				if (string.Compare(keyword, IfcUnitaryControlElementType.mKW, true) == 0)
					return IfcUnitaryControlElementType.Parse(str);
				if (string.Compare(suffix, "UNITARYEQUIPMENT", true) == 0)
					return IfcUnitaryEquipment.Parse(str);
				if (string.Compare(keyword, IfcUnitaryEquipmentType.mKW, true) == 0)
					return IfcUnitaryEquipmentType.Parse(str);
				if (string.Compare(suffix, "UNITASSIGNMENT", true) == 0)
					return IfcUnitAssignment.Parse(str);
				if (string.Compare(keyword, IfcUShapeProfileDef.mKW, true) == 0)
					return IfcUShapeProfileDef.Parse(str);

				if (string.Compare(suffix, "VALVE", true) == 0)
					return IfcValve.Parse(str);
				if (string.Compare(keyword, IfcValveType.mKW, true) == 0)
					return IfcValveType.Parse(str);
				if (string.Compare(keyword, IfcVector.mKW, true) == 0)
					return IfcVector.Parse(str);
				if (string.Compare(keyword, IfcVertexPoint.mKW, true) == 0)
					return IfcVertexPoint.Parse(str);
				if (string.Compare(keyword, IfcVoidingFeature.mKW, true) == 0)
					return IfcVoidingFeature.Parse(str,schema);
				if (string.Compare(keyword, IfcVirtualElement.mKW, true) == 0)
					return IfcVirtualElement.Parse(str);
				if (string.Compare(keyword, IfcVirtualGridIntersection.mKW, true) == 0)
					return IfcVirtualGridIntersection.Parse(str);
				if (string.Compare(keyword, IfcWall.mKW, true) == 0)
					return IfcWall.Parse(str,schema);
				if (string.Compare(keyword, IfcWallType.mKW, true) == 0)
					return IfcWallType.Parse(str);
				if (string.Compare(suffix, "WALLSTANDARDCASE", true) == 0)
					return IfcWallStandardCase.Parse(str,schema);
				if (string.Compare(suffix, "WASTETERMINAL", true) == 0)
					return IfcWasteTerminal.Parse(str);
				if (string.Compare(keyword, IfcWasteTerminalType.mKW, true) == 0)
					return IfcWasteTerminalType.Parse(str);
				if (string.Compare(keyword, IfcWindow.mKW, true) == 0)
					return IfcWindow.Parse(str,schema);
				if (string.Compare(keyword, IfcWindowLiningProperties.mKW, true) == 0)
					return IfcWindowLiningProperties.Parse(str, schema);
				if (string.Compare(keyword, IfcWindowPanelProperties.mKW, true) == 0)
					return IfcWindowPanelProperties.Parse(str);
				if (string.Compare(keyword, IfcWindowStandardCase.mKW, true) == 0)
					return IfcWindowStandardCase.Parse(str);
				if (string.Compare(keyword, IfcWindowStyle.mKW, true) == 0)
					return IfcWindowStyle.Parse(str);
				if (string.Compare(keyword, IfcWindowType.mKW, true) == 0)
					return IfcWindowType.Parse(str);
				if (string.Compare(keyword, IfcWorkCalendar.mKW, true) == 0)
					return IfcWorkCalendar.Parse(str);
				if (string.Compare(keyword, IfcWorkPlan.mKW, true) == 0)
					return IfcWorkPlan.Parse(str);
				if (string.Compare(keyword, IfcWorkSchedule.mKW, true) == 0)
					return IfcWorkSchedule.Parse(str,schema);
				if (string.Compare(keyword, IfcWorkTime.mKW, true) == 0)
					return IfcWorkTime.Parse(str);
				if (string.Compare(suffix, "ZONE", true) == 0)
					return IfcZone.Parse(str);
				if (string.Compare(suffix, "ZSHAPEPROFILEDEF", true) == 0)
					return IfcZShapeProfileDef.Parse(str);
				if (string.Compare(keyword, Ifc2dCompositeCurve.mKW, true) == 0)
					return Ifc2dCompositeCurve.Parse(str);
			}
			#endregion

			return null;
		}

		internal static IfcColour parseColour(string str)
		{
			string kw = "", def = "";
			int id = 0;
			ParserIfc.GetKeyWord(str, out id, out kw, out def);
			if (string.IsNullOrEmpty(kw))
				return null;
			if (string.Compare(kw, IfcColourRgb.mKW, false) == 0)
				return IfcColourRgb.Parse(str);
			if (string.Compare(kw, IfcDraughtingPreDefinedColour.mKW, false) == 0)
				return IfcDraughtingPreDefinedColour.Parse(str);
			return null;
		}
		internal static IfcColourOrFactor parseColourOrFactor(string str)
		{
			if (str[0] == '#')
				return null;
			string kw = "", def = "";
			int id = 0;
			ParserIfc.GetKeyWord(str, out id, out kw, out def);
			if (string.IsNullOrEmpty(kw))
				return null;
			if (string.Compare(kw, IfcColourRgb.mKW, false) == 0)
				return IfcColourRgb.Parse(str);
			return new IfcNormalisedRatioMeasure(ParserSTEP.ParseDouble(def));
		}
		internal static IfcDerivedMeasureValue parseDerivedMeasureValue(string str)
		{
			int len = str.Length;
			if (str.EndsWith(")"))
				len--;
			int icounter = 0;
			char c = str[icounter];
			while (!char.IsDigit(c) && icounter < str.Length)
				c = str[icounter++];
			if (icounter == str.Length)
				return null;
			icounter--;
			if (icounter > 1)
			{
				string kw = str.Substring(0, icounter - 1);
				double val = 0;
				if (double.TryParse(str.Substring(icounter, len - icounter), out val))
				{
					if (string.Compare(kw, IfcVolumetricFlowRateMeasure.mKW, true) == 0)
						return new IfcVolumetricFlowRateMeasure(val);
					if (string.Compare(kw, IfcThermalTransmittanceMeasure.mKW, true) == 0)
						return new IfcThermalTransmittanceMeasure(val);
					//IfcThermalResistanceMeasure, 
					//IfcThermalAdmittanceMeasure,  
					if (string.Compare(kw, IfcPressureMeasure.mKW, true) == 0)
						return new IfcPressureMeasure(val);
					//IfcPowerMeasure, 
					//IfcMassFlowRateMeasure, 
					if (string.Compare(kw, IfcMassDensityMeasure.mKW, true) == 0)
						return new IfcMassDensityMeasure(val);
					/*IfcLinearVelocityMeasure, 
					IfcKinematicViscosityMeasure, 
					IfcIntegerCountRateMeasure, 
					IfcHeatFluxDensityMeasure, 
					IfcFrequencyMeasure, 
					IfcEnergyMeasure, 
					IfcElectricVoltageMeasure, 
					, */
					if (string.Compare(kw, IfcDynamicViscosityMeasure.mKW, true) == 0)
						return new IfcDynamicViscosityMeasure(val);
					//	if (string.Compare(kw, IfcCompoundPlaneAngleMeasure.mKW, true) == 0)
					//		return new IfcCompoundPlaneAngleMeasure(val);

					/*IfcAngularVelocityMeasure, 
					IfcThermalConductivityMeasure, */
					if (string.Compare(kw, IfcMolecularWeightMeasure.mKW, true) == 0)
						return new IfcMolecularWeightMeasure(val);
					/*IfcVaporPermeabilityMeasure, 
					IfcMoistureDiffusivityMeasure, 
					IfcIsothermalMoistureCapacityMeasure, 
					IfcSpecificHeatCapacityMeasure, */
					if (string.Compare(kw, IfcMonetaryMeasure.mKW, true) == 0)
						return new IfcMonetaryMeasure(val);
					/*IfcMagneticFluxDensityMeasure, 
					IfcMagneticFluxMeasure, 
					IfcLuminousFluxMeasure, */
					if (string.Compare(kw, IfcForceMeasure.mKW, true) == 0)
						return new IfcForceMeasure(val);
					/*IfcInductanceMeasure, 
					IfcIlluminanceMeasure, 
					IfcElectricResistanceMeasure, 
					IfcElectricConductanceMeasure, 
					IfcElectricChargeMeasure, 
					IfcDoseEquivalentMeasure, 
					IfcElectricCapacitanceMeasure, 
					IfcAbsorbedDoseMeasure, 
					IfcRadioActivityMeasure, 
					IfcRotationalFrequencyMeasure, 
					IfcTorqueMeasure, 
					IfcAccelerationMeasure, 
					IfcLinearForceMeasure, */
					if (string.Compare(kw, IfcLinearStiffnessMeasure.mKW, true) == 0)
						return new IfcLinearStiffnessMeasure(val);
					//IfcModulusOfSubgradeReactionMeasure, 
					if (string.Compare(kw, IfcModulusOfElasticityMeasure.mKW, true) == 0)
						return new IfcModulusOfElasticityMeasure(val);
					/*IfcMomentOfInertiaMeasure, 
					IfcPlanarForceMeasure,  */
					if (string.Compare(kw, IfcRotationalStiffnessMeasure.mKW, true) == 0)
						return new IfcRotationalStiffnessMeasure(val);
					/*IfcShearModulusMeasure, 
					IfcLinearMomentMeasure, 
					IfcLuminousIntensityDistributionMeasure, 
					IfcCurvatureMeasure, */
					if (string.Compare(kw, IfcMassPerLengthMeasure.mKW, true) == 0)
						return new IfcMassPerLengthMeasure(val);

					/*IfcModulusOfLinearSubgradeReactionMeasure, 
					IfcModulusOfRotationalSubgradeReactionMeasure, 
					IfcRotationalMassMeasure, 
					IfcSectionalAreaIntegralMeasure, 
					IfcSectionModulusMeasure, 
					IfcTemperatureGradientMeasure, 
					, */
					if (string.Compare(kw, IfcThermalExpansionCoefficientMeasure.mKW, true) == 0)
						return new IfcThermalExpansionCoefficientMeasure(val);
					if (string.Compare(kw, IfcWarpingConstantMeasure.mKW, true) == 0)
						return new IfcWarpingConstantMeasure(val);
					if (string.Compare(kw, IfcWarpingMomentMeasure.mKW, true) == 0)
						return new IfcWarpingMomentMeasure(val);
					/*IfcSoundPowerMeasure, 
					IfcSoundPressureMeasure, 
					IfcHeatingValueMeasure, 
					IfcPHMeasure, 
					IfcIonConcentrationMeasure, 
					IfcTemperatureRateOfChangeMeasure, 
					IfcAreaDensityMeasure, 
					IfcSoundPowerLevelMeasure, 
					IfcSoundPressureLevelMeasure);*/
				}
			}
			return null;
		}
		internal static IfcMeasureValue parseMeasureValue(string str)
		{
			int len = str.Length;
			if (str.EndsWith(")"))
				len--;
			int icounter = 0;
			char c = str[icounter];
			while (!char.IsDigit(c) && icounter < str.Length)
				c = str[icounter++];
			if (icounter == str.Length)
				return null;
			icounter--;
			if (icounter > 1)
			{
				string kw = str.Substring(0, icounter - 1);
				double val = 0;
				int i = 0;
				if (int.TryParse(str.Substring(icounter, len - icounter), out i))
				{
					if (string.Compare(kw, IfcCountMeasure.mKW, true) == 0)
						return new IfcCountMeasure(i);
				}
				if (double.TryParse(str.Substring(icounter, len - icounter), out val))
				{
					if (string.Compare(kw, IfcVolumeMeasure.mKW, true) == 0)
						return new IfcVolumeMeasure(val);
					if (string.Compare(kw, IfcTimeMeasure.mKW, true) == 0)
						return new IfcTimeMeasure(val);
					if (string.Compare(kw, IfcThermodynamicTemperatureMeasure.mKW, true) == 0)
						return new IfcThermodynamicTemperatureMeasure(val);
					//IfcSolidAngleMeasure, */
					if (string.Compare(kw, IfcPositiveRatioMeasure.mKW, true) == 0)
						return new IfcPositiveRatioMeasure(val);
					if (string.Compare(kw, IfcRatioMeasure.mKW, true) == 0)
						return new IfcRatioMeasure(val);
					//IfcPositivePlaneAngleMeasure,
					if (string.Compare(kw, IfcPlaneAngleMeasure.mKW, true) == 0)
						return new IfcPlaneAngleMeasure(val);
					//if (string.Compare(kw, IfcParameterValue.mKW, true) == 0)
					//	return new IfcParameterValue(val);
					//	if (string.Compare(kw, IfcNumericMeasure.mKW, true) == 0)
					//	return new IfcNumericMeasure(val); 
					if (string.Compare(kw, IfcMassMeasure.mKW, true) == 0)
						return new IfcMassMeasure(val);
					if (string.Compare(kw, IfcPositiveLengthMeasure.mKW, true) == 0)
						return new IfcPositiveLengthMeasure(val);
					if (string.Compare(kw, IfcLengthMeasure.mKW, true) == 0)
						return new IfcLengthMeasure(val);
					//IfcElectricCurrentMeasure, 


					//IfcContextDependentMeasure, 
					if (string.Compare(kw, IfcAreaMeasure.mKW, true) == 0)
						return new IfcAreaMeasure(val);
					//IfcAmountOfSubstanceMeasure, 
					//IfcLuminousIntensityMeasure, 
					if (string.Compare(kw, IfcNormalisedRatioMeasure.mKW, true) == 0)
						return new IfcNormalisedRatioMeasure(val);
					//IfcComplexNumber, 
					//IfcNonNegativeLengthMeasure);
				}
				if (string.Compare(kw, IfcDescriptiveMeasure.mKW, true) == 0)
					return new IfcDescriptiveMeasure(str.Substring(icounter, len - icounter));
			}
			return null;
		}
		internal static IfcSimpleValue parseSimpleValue(string str)
		{
			if (str.StartsWith("IFCBOOLEAN("))
				return new IfcBoolean(string.Compare(str.Substring(11, str.Length - 12), ".T.") == 0);
			if (str.StartsWith("IFCIDENTIFIER("))
				return new IfcIdentifier(str.Substring(15, str.Length - 17));
			if (str.StartsWith("IFCINTEGER("))
				return new IfcInteger(int.Parse(str.Substring(11, str.Length - 12)));
			if (str.StartsWith("IFCLABEL("))
			{
				string s = str.Substring(9, str.Length - 10).Replace("'", "");
				return new IfcLabel((s == "$" || string.IsNullOrEmpty(s) ? "DEFAULT" : s));
			}
			if (str.StartsWith("IFCLOGICAL("))
			{
				string s = str.Substring(11, str.Length - 12);
				IfcLogicalEnum l = IfcLogicalEnum.UNKNOWN;
				if (s == ".T.")
					l = IfcLogicalEnum.TRUE;
				else if (s == ".F.")
					l = IfcLogicalEnum.FALSE;
				return new IfcLogical(l);
			}
			if (str.StartsWith("IFCREAL("))
				return new IfcReal(ParserSTEP.ParseDouble(str.Substring(8, str.Length - 9)));
			if (str.StartsWith("IFCTEXT("))
			{
				string s = str.Substring(8, str.Length - 9).Replace("'", "");
				return new IfcText((s == "$" || string.IsNullOrEmpty(s) ? "DEFAULT" : s));
			}
			int i = 0;
			if (int.TryParse(str, out i))
				return new IfcInteger(i);
			double d = 0;
			if (double.TryParse(str, out d))
				return new IfcReal(d);
			if (str == ".T.")
				return new IfcBoolean(true);
			if (str == ".F.")
				return new IfcBoolean(false);
			if (str == ".U.")
				return new IfcLogical(IfcLogicalEnum.UNKNOWN);
			return null;
		}
		internal static IfcValue parseValue(string str)
		{
			IfcMeasureValue sv = parseMeasureValue(str);
			if (sv != null)
				return sv;
			IfcDerivedMeasureValue mv = parseDerivedMeasureValue(str);
			if (mv != null)
				return mv;
			return parseSimpleValue(str);
		}
		internal static bool TryGetDouble(IfcValue v, out double val)
		{
			IfcReal r = v as IfcReal;
			if (r != null)
			{
				val = r.mValue;
				return true;
			}
			IfcInteger i = v as IfcInteger;
			if (i != null)
			{
				val = i.mValue;
				return true;
			}
			IfcPositiveLengthMeasure plm = v as IfcPositiveLengthMeasure;
			if (plm != null)
			{
				val = plm.mValue;
				return true;
			}
			IfcDynamicViscosityMeasure dvm = v as IfcDynamicViscosityMeasure;
			if (dvm != null)
			{
				val = dvm.mValue;
				return true;
			}
			IfcMassDensityMeasure mdm = v as IfcMassDensityMeasure;
			if (mdm != null)
			{
				val = mdm.mValue;
				return true;
			}
			IfcModulusOfElasticityMeasure mem = v as IfcModulusOfElasticityMeasure;
			if (mem != null)
			{
				val = mem.mValue;
				return true;
			}
			IfcPositiveRatioMeasure prm = v as IfcPositiveRatioMeasure;
			if (prm != null)
			{
				val = prm.mValue;
				return true;
			}
			IfcThermalExpansionCoefficientMeasure tec = v as IfcThermalExpansionCoefficientMeasure;
			if (tec != null)
			{
				val = tec.mValue;
				return true;
			}
			val = 0;
			return false;
		}


		//http://madskristensen.net/post/A-shorter-and-URL-friendly-GUID.aspx
		/// <summary>
		/// Conversion methods between an IFC 
		/// encoded GUID string and a .NET GUID.
		/// This is a translation of the C code 
		/// found here: 
		/// http://www.iai-tech.org/ifc/IFC2x3/TC1/html/index.htm
		/// </summary>
		/// 
		 
		#region Private Members
		/// <summary>
		/// The replacement table
		/// </summary>
		private static readonly char[] base64Chars = new char[]
    { '0','1','2','3','4','5','6','7','8','9'
    , 'A','B','C','D','E','F','G','H','I','J'
    , 'K','L','M','N','O','P','Q','R','S','T'
    , 'U','V','W','X','Y','Z','a','b','c','d'
    , 'e','f','g','h','i','j','k','l','m','n'
    , 'o','p','q','r','s','t','u','v','w','x'
    , 'y','z','_','$' };

		/// <summary>
		/// Conversion of an integer into characters 
		/// with base 64 using the table base64Chars
		/// </summary>
		/// <param name="number">The number to convert</param>
		/// <param name="result">The result char array to write to</param>
		/// <param name="start">The position in the char array to start writing</param>
		/// <param name="len">The length to write</param>
		/// <returns></returns>
		static void cv_to_64(uint number, ref char[] result, int start, int len)
		{
			uint act;
			int iDigit, nDigits;

			Debug.Assert(len <= 4);
			act = number;
			nDigits = len;

			for (iDigit = 0; iDigit < nDigits; iDigit++)
			{
				result[start + len - iDigit - 1] = base64Chars[(int)(act % 64)];
				act /= 64;
			}
			Debug.Assert(act == 0, "Logic failed, act was not null: " + act.ToString());
			return;
		}

		/// <summary>
		/// The reverse function to calculate 
		/// the number from the characters
		/// </summary>
		/// <param name="str">The char array to convert from</param>
		/// <param name="start">Position in array to start read</param>
		/// <param name="len">The length to read</param>
		/// <returns>The calculated nuber</returns>
		static uint cv_from_64(char[] str, int start, int len)
		{
			int i, j, index;
			uint res = 0;
			Debug.Assert(len <= 4);

			for (i = 0; i < len; i++)
			{
				index = -1;
				for (j = 0; j < 64; j++)
				{
					if (base64Chars[j] == str[start + i])
					{
						index = j;
						break;
					}
				}
				Debug.Assert(index >= 0);
				res = res * 64 + ((uint)index);
			}
			return res;
		}
		#endregion // Private Members

		#region Conversion Methods
		/// <summary>
		/// Reconstruction of the GUID 
		/// from an IFC GUID string (base64)
		/// </summary>
		/// <param name="guid">The GUID string to convert. Must be 22 characters int</param>
		/// <returns>GUID correspondig to the string</returns>
		public static Guid DecodeGlobalID(string guid)
		{
			try
			{
				if (guid.Length == 22)
				{
					uint[] num = new uint[6];
					char[] str = guid.ToCharArray();
					int n = 2, pos = 0, i;
					for (i = 0; i < 6; i++)
					{
						num[i] = cv_from_64(str, pos, n);
						pos += n; n = 4;
					}
					int a = (int)((num[0] * 16777216 + num[1]));
					short b = (short)(num[2] / 256);
					short c = (short)((num[2] % 256) * 256 + num[3] / 65536);
					byte[] d = new byte[8];
					d[0] = Convert.ToByte((num[3] / 256) % 256);
					d[1] = Convert.ToByte(num[3] % 256);
					d[2] = Convert.ToByte(num[4] / 65536);
					d[3] = Convert.ToByte((num[4] / 256) % 256);
					d[4] = Convert.ToByte(num[4] % 256);
					d[5] = Convert.ToByte(num[5] / 65536);
					d[6] = Convert.ToByte((num[5] / 256) % 256);
					d[7] = Convert.ToByte(num[5] % 256);

					return new Guid(a, b, c, d);
				}
			}
			catch (Exception) { }
			return Guid.Empty;
		}

		/// <summary>
		/// Conversion of a GUID to a string 
		/// representing the GUID 
		/// </summary>
		/// <param name="guid">The GUID to convert</param>
		/// <returns>IFC (base64) encoded GUID string</returns>
		public static string EncodeGuid(Guid guid)
		{
			uint[] num = new uint[6];
			char[] str = new char[22];
			int i, n;
			byte[] b = guid.ToByteArray();

			// Creation of six 32 Bit integers from the components of the GUID structure
			num[0] = (uint)(BitConverter.ToUInt32(b, 0) / 16777216);
			num[1] = (uint)(BitConverter.ToUInt32(b, 0) % 16777216);
			num[2] = (uint)(BitConverter.ToUInt16(b, 4) * 256 + BitConverter.ToInt16(b, 6) / 256);
			num[3] = (uint)((BitConverter.ToUInt16(b, 6) % 256) * 65536 + b[8] * 256 + b[9]);
			num[4] = (uint)(b[10] * 65536 + b[11] * 256 + b[12]);
			num[5] = (uint)(b[13] * 65536 + b[14] * 256 + b[15]);

			// Conversion of the numbers into a system using a base of 64
			n = 2;
			int pos = 0;
			for (i = 0; i < 6; i++)
			{
				cv_to_64(num[i], ref str, pos, n);
				pos += n; n = 4;
			}
			return new String(str);
		}
		#endregion // Conversion Methods

	 
	}
}
