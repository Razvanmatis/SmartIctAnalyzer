using System;
using System.Collections.Generic;
using System.Text;

namespace ProMik.SmartIct.Interfaces.PcbInvestigator.Enums
{
    public enum PcbComponentType
    {
        /// <summary>
        /// IC
        /// </summary>
        IC = 0,

        /// <summary>
        /// Connector
        /// </summary>
        Connector,

        /// <summary>
        /// Capacitor
        /// </summary>
        Capacitor,

        /// <summary>
        /// Induction
        /// </summary>
        Induction,

        /// <summary>
        /// Resistor
        /// </summary>
        Resistor,

        /// <summary>
        /// TestPoint
        /// </summary>
        TestPoint,

        /// <summary>
        /// Undefined
        /// </summary>
        Undefined,

        /// <summary>
        /// DirectGnd
        /// </summary>
        DirectGnd,

        /// <summary>
        /// DirectPower
        /// </summary>
        DirectPower
    }
}
