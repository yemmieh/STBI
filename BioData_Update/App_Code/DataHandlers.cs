using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace BioData_Update.App_Code {
    class DataHandlers {

        public DataHandlers() {}

        public const string APP_ID="PASSPORT_BIODATA";

        public static DataTable ToDataTable<T>( IEnumerable<T> data) {
            
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            
            foreach (PropertyDescriptor prop in properties) { 
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }
            
            foreach (T item in data) {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties) { 
                    Debug.WriteLine(prop.Name);   
                    Debug.WriteLine(prop.GetValue(item)); 
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                    //Debug.WriteLine(row[prop.Name]);
                }
                table.Rows.Add(row);
            }
            return table;
        }
        public static XDocument ToXDocument( XElement srcXElement ) {
            
            StringBuilder stringBuilder = new StringBuilder();
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.OmitXmlDeclaration = true;
            xmlWriterSettings.Indent = true;
            
            using (XmlWriter xmlWriter = XmlWriter.Create( stringBuilder , xmlWriterSettings )) {
                srcXElement.WriteTo( xmlWriter );
            }
            Console.WriteLine(stringBuilder.ToString());

            XDocument xDocument = XDocument.Parse("<?xml version=\"1.0\" encoding=\"utf-8a\" ?>"+stringBuilder.ToString ());
            return xDocument;
        }

        /*private static DateTime ConvertToDateTime(string value) {
            DateTime convertedDate = new DateTime();
            try {
                convertedDate = Convert.ToDateTime(value);
                Console.WriteLine("'{0}' converts to {1} {2} time.", value, convertedDate, convertedDate.Kind.ToString());
            } catch (FormatException) {
                convertedDate = null;
                Console.WriteLine("'{0}' is not in the proper format.", value);
            }

            return convertedDate;
        }*/
    }
}
