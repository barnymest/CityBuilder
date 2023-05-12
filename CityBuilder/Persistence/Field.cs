using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityBuilder.Persistence
{
    public class Field
    {
        private int row;
        private int column;
        private FieldTypes type;
        private ZoneTypes zone;
        private double fireRisk;
        private int buildingID;
        private double publicSecurity;
                
        public Field(int row, int column, FieldTypes _type, ZoneTypes _zone)
        {
            Row = row;
            Column = column;
            type = _type;
            zone = _zone;
            buildingID = -1;
        }

        public int Row { get { return row; } set { row = value; } }
        public int Column { get { return column; } set { column = value; } }
        public FieldTypes Type { get { return type; } set { type = value; } }
        public ZoneTypes Zone { get { return zone; } set { zone = value; } }
        public double FireRisk { get { return fireRisk; } set { fireRisk = value; } }
        public int BuildingID { get { return buildingID; } set { buildingID = value; } }
        public double PublicSecurity { get { return publicSecurity; } set { publicSecurity = value; } }
        public int ZonePlaced { get; set; }
    }
}
