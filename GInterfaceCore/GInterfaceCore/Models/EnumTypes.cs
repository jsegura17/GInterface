﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GInterfaceCore.Models
{
    /// <summary>
	/// Summary description for EnumTypes.
	/// </summary>
	public class EnumTypes
    {
        public enum DocumentType
        {
            Generico = 0,
            Picklist = 1,
            TipoDocumento_02 = 2,
            TipoDocumento_03 = 3,
            FormatoIngreso = 4,
            IngresoPorDevolucionCliente = 5,
            IngresoPorDevolucionRetorno = 6,
            IngresoPorConduces = 7,
            
        }

        public enum UserType
        {
            Generico = 0,
            Codirecta=1,
            Dinor=2,
            Norteña=3,
            Merlodist=4,
            Noroestana=5,

        }
        public enum Entry
        {
            Ingreso = 1,
            Egreso = 2,
            Conduce = 3,

        }
        public enum ConfigArea
        {
            FuenteDeDatos,
            CamposDeOrigen,
            FuenteDeDestino,
            CamposDeMapeo,
            Documento
        }

        public enum LastConnectionStatus
        {
            CONNECTED,
            DISCONNECTED
        }

        public enum TransactionType
        {
            /// <summary>Receiving a Transaction Part</summary>
            RECEIVE,
            /// <summary>Returning a Transaction Part</summary>
            RETURN,
            /// <summary>NULL OR NONE</summary>
            NONE
        }

        public enum AccessType
        {
            User = 1,
            Admin = 2
        }

        /// <summary>
        /// Type of receive for a location and part category
        /// </summary>
        /// TODO: Poner aca los tipos de registros a Recibir
        public enum ReceiveType
        {
            BRCVREAD,
            /// <summary> Transaction Read</summary>
            BREAD,
            /// <summary> Transaction RETURN</summary>
            LCREAD,
            /// <summary> NULL OR NONE</summary>
            NONE
        }


        /// <summary>
        /// The types of transaction processes that can occur
        /// Corresponds to the transTypeCode in Menu.xml
        /// or could be the ReceiveTypeID, MoveInTypeID, MoveOutTypeID,
        /// RoundReadTypeID, ReturnTypeID in tblAreaLocation
        /// or the ReceiveTypeID, MoveInTypeID, MoveOutTypeID,
        /// RoundReadTypeID, ReturnTypeID in tblAreaLocPartCatTransType
        /// </summary>
        public enum TransactionProcessCode
        {
            ///<summary>Returnable - Receive</summary>
            RRCV,
            ///<summary>Returnable - Return</summary>
            RRET,
            ///<summary>Disposable - Receive</summary>
            DRCV,
            /// <summary> NULL OR NONE</summary>
            NONE
        }
               
        public enum DataSource
        {
            /// <summary>Data from SQL CE database</summary>
            SQLCE,
            /// <summary>Data from Xml file</summary>
            XML
        }

        public enum TransactionStatus
        {
            nothing,
            Pending,
            Processing,
            Completed,
            Canceled,
            Rejected,
            Reserved,
            RejectedCollection,
            Commited,
            NotCommited,
            Omit,
            Error
        }

        public enum TransactionTask
        {
            /// <summary>Read Token of User</summary>
            GET_TOKEN,
            /// <summary>Read IMEI from HH</summary>
            GET_IMEI,
            /// <summary>Read Login from HH</summary>
            GET_LOGIN,
            /// <summary>Read LogOUT from HH</summary>
            GET_LOGOUT,
            /// <summary>Execute PUSH process</summary>
            GET_PUSH,
            /// <summary>Execute PULL process</summary>
            GET_PULL,
            /// <summary>Execute Config process</summary>
            GET_CONFIG,
            /// <summary>Go Home </summary>
            GET_HOME,
            /// <summary>Show Main Menu</summary>
            GET_MAIN_MENU,
            /// <summary>Update Footer Info </summary>
            GET_UPDATE_FOOTER_INFO
        }

        public enum IssueState
        {
            Open,
            Closed,
            All
        }

        public enum MenuItems
        {
            [Description("Home")]
            Home = 1,
            [Description("Manejo de Documentos")]
            Documentos = 2,
            [Description("Config")]
            Config = 3,
            [Description("Registrar")]
            Registrar = 4,
            [Description("Login")]
            Login = 4,
        }
    }
}
