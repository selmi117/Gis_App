using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;


namespace MapasGoogle
{
    public partial class Form1 : Form
    {
        string ConnectString = "datasource = localhost; port = 3306; username = root; password=; database = réseau";


        GMarkerGoogle marker;
        GMapOverlay markerOverlay;
        DataTable dt;

        // Route automatisée (adresse)
        bool trazarRuta = false;
        int ContadorIndicadoresRuta = 0;
        PointLatLng inicio;
        PointLatLng final;

        int filaSeleccionada = 0;
        double LatInicial = 36.8946118458878;
        double LngInicial = 10.1854956150055;
        /* private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView1.Rows.Count; ++i)
            {
                string name = Convert.ToString(dataGridView1.Rows[i].Cells[0].Value);
                double lat = Convert.ToDouble(dataGridView1.Rows[i].Cells[1].Value);
                double lng = Convert.ToDouble(dataGridView1.Rows[i].Cells[2].Value);
                




                gMapControl1.MapProvider = GMapProviders.GoogleMap;
                GMapOverlay markersOverlay = new GMapOverlay("markers");
                GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(lat, lng), GMarkerGoogleType.lightblue);

                markersOverlay.Markers.Add(marker);
                marker.ToolTipMode = MarkerTooltipMode.Always;
                //marker.ToolTip = new GMapRoundedToolTip(marker);
                marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                marker.ToolTipText = "name: " + name + "\n LAT:" + lat + "\n long: " + lng;
                gMapControl1.Overlays.Add(markersOverlay);
            }
        }
        */

        public Form1()
        {
            InitializeComponent();

        }

        private void DBConnection()
        {
            string mycnx = ConnectString;
            MySqlConnection DBConnect = new MySqlConnection(mycnx);
            try
            {
                DBConnect.Open();
                MessageBox.Show("Ok you are connected");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {


            dt = new DataTable();
            dt.Columns.Add(new DataColumn("Description", typeof(string)));
            dt.Columns.Add(new DataColumn("Lat", typeof(double)));
            dt.Columns.Add(new DataColumn("Long", typeof(double)));

            //insertion des données dans la DT à afficher dans la liste

            dt.Rows.Add("Site_Name", LatInicial, LngInicial);
            dataGridView1.DataSource = dt;

            //désactiver les colonnes lat et long
            dataGridView1.Columns[1].Visible = false;
            dataGridView1.Columns[2].Visible = false;

            //Création des dimensions du GMAPCONTROL(outil)
            gMapControl1.DragButton = MouseButtons.Left;
            gMapControl1.CanDragMap = true;
            gMapControl1.MapProvider = GMapProviders.GoogleMap;
            gMapControl1.Position = new PointLatLng(LatInicial, LngInicial);
            gMapControl1.MinZoom = 0;
            gMapControl1.MaxZoom = 24;
            gMapControl1.Zoom = 9;
            gMapControl1.AutoScroll = true;
            gMapControl1.ShowCenter = false;
            /*
            //highlighter
            markerOverlay = new GMapOverlay("highlighter");
            marker = new GMarkerGoogle(new PointLatLng(LatInicial, LngInicial), GMarkerGoogleType.blue);
            markerOverlay.Markers.Add(marker);//ajouts à la carte

            //ajoute une info-bulle de texte aux marqueurs
            marker.ToolTipMode = MarkerTooltipMode.Always;
            marker.ToolTipText = string.Format("Site_Name:\n Latitude:{0}\n Longueur:{1}", LatInicial, LngInicial);
            */
            gMapControl1.Overlays.Add(markerOverlay);


        }

        private void SelecionarRegistro(object sender, DataGridViewCellMouseEventArgs e)
        {
            filaSeleccionada = e.RowIndex;
            txtDescripcion.Text = dataGridView1.Rows[filaSeleccionada].Cells[0].Value.ToString();
            txtlatitud.Text = dataGridView1.Rows[filaSeleccionada].Cells[1].Value.ToString();
            txtlongitud.Text = dataGridView1.Rows[filaSeleccionada].Cells[2].Value.ToString();

            marker.Position = new PointLatLng(Convert.ToDouble(txtlatitud.Text), Convert.ToDouble(txtlongitud.Text));
            gMapControl1.Position = marker.Position;

        }

        private void gMapControl1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            double lat = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lat;
            double lng = gMapControl1.FromLocalToLatLng(e.X, e.Y).Lng;

            txtlatitud.Text = lat.ToString();
            txtlongitud.Text = lng.ToString();

            marker.Position = new PointLatLng(lat, lng);
            marker.ToolTipText = string.Format("Site_Name:\n Latitude:{0}\n Longueur:{1}", lat, lng);

            CrearDireccionTrazarRuta(lat, lng);

        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            dt.Rows.Add(txtDescripcion.Text, txtlatitud.Text, txtlongitud.Text);

        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.RemoveAt(filaSeleccionada);
        }



        private void btnPolygon_Click(object sender, EventArgs e)
        {
            GMapOverlay Poligono = new GMapOverlay("POligone");
            List<PointLatLng> puntos = new List<PointLatLng>();

            double lng, lat;
            for (int filas = 0; filas < dataGridView1.Rows.Count; filas++)
            {
                lat = Convert.ToDouble(dataGridView1.Rows[filas].Cells[1].Value);
                lng = Convert.ToDouble(dataGridView1.Rows[filas].Cells[2].Value);
                puntos.Add(new PointLatLng(lat, lng));
            }

            GMapPolygon poligonoPuntos = new GMapPolygon(puntos, "Polígone");
            Poligono.Polygons.Add(poligonoPuntos);
            gMapControl1.Overlays.Add(Poligono);
            gMapControl1.Zoom = gMapControl1.Zoom + 1;
            gMapControl1.Zoom = gMapControl1.Zoom - 1;

        }

        private void btnRuta_Click(object sender, EventArgs e)
        {

            GMapOverlay Ruta = new GMapOverlay("Capaitin");

            List<PointLatLng> puntos = new List<PointLatLng>();
            double lng, lat;
            for (int filas = 0; filas < dataGridView1.Rows.Count; filas++)
            {
                lat = Convert.ToDouble(dataGridView1.Rows[filas].Cells[1].Value);
                lng = Convert.ToDouble(dataGridView1.Rows[filas].Cells[2].Value);
                puntos.Add(new PointLatLng(lat, lng));
            }

            GMapRoute PuntosRuta = new GMapRoute(puntos, "itineraire");
            Ruta.Routes.Add(PuntosRuta);
            gMapControl1.Overlays.Add(Ruta);

            gMapControl1.Zoom = gMapControl1.Zoom + 1;
            gMapControl1.Zoom = gMapControl1.Zoom - 1;


        }

        public void CrearDireccionTrazarRuta(double lat, double lng)
        {
            if (trazarRuta)
            {
                switch (ContadorIndicadoresRuta)
                {
                    case 0:
                        ContadorIndicadoresRuta++;
                        inicio = new PointLatLng(lat, lng);
                        break;
                    case 1:
                        ContadorIndicadoresRuta++;
                        final = new PointLatLng(lat, lng);
                        GDirections direccion;
                        var RutasDireccion = GMapProviders.GoogleMap.GetDirections(out direccion, inicio, final, false, false, false, false, false);
                        GMapRoute RutaObtenida = new GMapRoute(direccion.Route, "emplacement du chemin");
                        GMapOverlay CapaRutas = new GMapOverlay("itinéraire");
                        CapaRutas.Routes.Add(RutaObtenida);
                        gMapControl1.Overlays.Add(CapaRutas);
                        gMapControl1.Zoom = gMapControl1.Zoom + 1;
                        gMapControl1.Zoom = gMapControl1.Zoom - 1;
                        ContadorIndicadoresRuta = 0;
                        trazarRuta = false;
                        break;
                }
            }
        }



        private void btnSat_Click(object sender, EventArgs e)
        {
            gMapControl1.MapProvider = GMapProviders.GoogleChinaSatelliteMap;

        }

        private void btnOriginal_Click(object sender, EventArgs e)
        {
            gMapControl1.MapProvider = GMapProviders.GoogleMap;
        }

        private void btnRelieve_Click(object sender, EventArgs e)
        {
            gMapControl1.MapProvider = GMapProviders.GoogleTerrainMap;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            trackZoom.Value = Convert.ToInt32(gMapControl1.Zoom);

        }

        private void trackZoom_ValueChanged(object sender, EventArgs e)
        {
            gMapControl1.Zoom = trackZoom.Value;
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void txtlatitud_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtlongitud_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

            //Connection à la BD
            string myConnection = ConnectString;
            MySqlConnection Mycon = new MySqlConnection(myConnection);
            Mycon.Open();

            //Recuperation des données dans le dataGrid
            MySqlCommand Sql = new MySqlCommand("SELECT Site_Name,Latitude,Longitude FROM deux_g", Mycon);
            MySqlDataReader dr;
            dr = Sql.ExecuteReader();
            dt = new DataTable();
            dt.Load(dr);
            dt.Rows.Add("Site_Name");
            dataGridView1.DataSource = dt;      
     
            //Affichage Sur Map avec Markers: Parcours du Tableau
            for (int i = 0; i < dataGridView1.Rows.Count; ++i)
            {

                string name = Convert.ToString(dataGridView1.Rows[i].Cells[0].Value);
                double lat = Convert.ToDouble(dataGridView1.Rows[i].Cells[1].Value);
                double lng = Convert.ToDouble(dataGridView1.Rows[i].Cells[2].Value);
                gMapControl1.MapProvider = GMapProviders.GoogleMap;

                //Creation du Marker
                GMapOverlay markersOverlay = new GMapOverlay("markers");
                GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(lat, lng), GMarkerGoogleType.lightblue);

                //Evenement OnMouseOver pour Markers
                markersOverlay.Markers.Add(marker);          
                marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                marker.ToolTipText = "name: " + name + "\n LAT:" + lat + "\n long: " + lng;
                gMapControl1.Overlays.Add(markersOverlay);
            }
        }
        
            
        }
    }
