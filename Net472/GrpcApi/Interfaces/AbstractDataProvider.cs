using System;
using System.Collections.Generic;
using System.Drawing;
using Interfaces.Helper;
using Interfaces.PcbInvestigator;
using Interfaces.PcbInvestigator.Enums;
using Interfaces.PcbInvestigator.Implementations;
using PCBI.Automation;
using PCBI.Automation.Interfaces;

namespace GrpcApi.Interfaces
{
    public abstract class AbstractDataProvider
    {
        public abstract IGrpcResult GetConvertedObjects(IList<InterfaceCMPObject> pcbObjects, IList<InterfaceNet> allNets, ref Func<IFunctionalAttributes, bool> isTestPoint);

        public abstract List<PcbTestObject> GetTransformedResults(List<INet> allNets, List<ICMPObject> listOfObjects);

        /// <summary>
        /// Get the components geometric attributes
        /// </summary>
        /// <param name="component">the component to create for</param>
        /// <returns>the geometric attributes</returns>
        protected static IGeometricAttributes GetComponentGeometricAttributes(InterfaceCMPObject component)
        {
            Rectangle bounds = new Rectangle(
                (int)component.Bounds.X,
                (int)component.Bounds.Y,
                (int)component.Bounds.Width,
                (int)component.Bounds.Height);
            PointF centerPoint = component.Position;
            float rotation = component.Rotation;
            return new GeometricAttributes(bounds, centerPoint, rotation, component.CompHEIGHT);
        }

        /// <summary>
        /// Get the geometric attributes for a Pin in dependency on the component
        /// </summary>
        /// <param name="pinToCheck">the pin to use</param>
        /// <param name="component">the componen to consider</param>
        /// <returns>the geometric attributes</returns>
        protected static IGeometricAttributes GetPinGeometricAttributes(InterfacePin pinToCheck, InterfaceCMPObject component)
        {
            RectangleF rectangleF = pinToCheck.GetBoundsD(component).ToRectangleF();
            PointF centerPoint = pinToCheck.GetIPinPositionGeometryD().ToPointF();
            Rectangle bounds = new Rectangle(
                (int)rectangleF.X,
                (int)rectangleF.Y,
                (int)rectangleF.Width,
                (int)rectangleF.Height);
            float rotation = 0f;
            return new GeometricAttributes(bounds, centerPoint, rotation, 0);
        }

        /// <summary>
        /// Get the pin type.
        /// </summary>
        /// <param name="type">the pint type by PCB investigator</param>
        /// <returns>my own pin type</returns>
        protected static PinComponentType GetPinType(IPin.PinType type)
        {
            return (PinComponentType)((int)type);
        }

        /// <summary>
        /// Get the functional attributes for a component
        /// </summary>
        /// <param name="component">the componente to create it for</param>
        /// <param name="isTestPoint">the is testpont func</param>
        /// <returns>the functional attributes</returns>
        protected static IFunctionalAttributes GetFunctionalAttributes(InterfaceCMPObject component, Func<IFunctionalAttributes, bool> isTestPoint)
        {
            PCBObjectType componentType = GetComponentType(component.Type);
            string reference = component.Ref;
            string partName = component.PartName;
            string layerName = component.LayerName;
            string packageName = component.UsedPackageName;
            string normalizedName = string.Empty;
            return new FunctionalAttributes(componentType, reference, partName, layerName, packageName, normalizedName, isTestPoint);
        }

        protected static PCBObjectType GetComponentType(IObjectType type)
        {
            if (type == IObjectType.Component)
            {
                return PCBObjectType.Component;
            }
            else if (type == IObjectType.Arc)
            {
                return PCBObjectType.Arc;
            }
            else if (type == IObjectType.Text)
            {
                return PCBObjectType.Text;
            }
            else if (type == IObjectType.Symbol)
            {
                return PCBObjectType.Symbol;
            }
            else if (type == IObjectType.Surface)
            {
                return PCBObjectType.Surface;
            }
            else if (type == IObjectType.Pad)
            {
                return PCBObjectType.Pad;
            }
            else if (type == IObjectType.Line)
            {
                return PCBObjectType.Line;
            }
            else
            {
                return PCBObjectType.Unknown;
            }
        }
    }
}
