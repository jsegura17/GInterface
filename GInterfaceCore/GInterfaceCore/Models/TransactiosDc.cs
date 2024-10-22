



namespace GInterfaceCore.Models
{
    public class TransactiosDc
    {

        public int ID { get; set; }  // ID autoincremental

        public int I_ID_CLIENT { get; set; }  // Llave foránea de I_CLIENT

        public int I_ID_SYSTEM { get; set; }  // Llave foránea de I_SYSTEM

        public Dictionary<int, string> I_ID_TYPEDOC { get; set; }  // Llave foránea de i_DocumentType

        public string I_JSONTEMPLATE { get; set; }  // Plantilla en formato JSON

        public string I_JSONDATA { get; set; }  // Datos en formato JSON

        public Status I_ID_STATUS { get; set; }  // Llave foránea de I_STATUS

        public DateOnly I_CREATED_DTM { get; set; }
    }
    /// creo que las tablas id tiene que ser un dictionary para luego clasificar
}
