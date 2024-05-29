using System.Collections.Generic;
using System.Text.Json;
using System.IO.Ports;
using System.Text.Json;
using System.Threading;

namespace ProyectoFinal
{
    public partial class Form1 : Form
    {
        private ListaDespacho listaDespacho;
       
        public Form1()
        {
            InitializeComponent();
            listaDespacho = new ListaDespacho();
            CargarDesdeArchivo();
           

        }



        public void CargarDesdeArchivo()
        {
            string nombreArchivo = "gasolinera.txt";
            if (File.Exists(nombreArchivo))
            {
                using (StreamReader leerArchivo = new StreamReader(nombreArchivo))
                {
                    string header = leerArchivo.ReadLine();
                    string linea;
                    while ((linea = leerArchivo.ReadLine()) != null)
                    {
                        string[] datos = linea.Split(';');
                        Despacho nuevoDespacho = new Despacho
                        {
                            Cliente = new Cliente
                            {
                                Nombre = datos[0],
                                Apellido = datos[1],
                                Nit = datos[2]
                            },
                            Bomba = new Bomba { CodigoBomba = datos[3] },
                            TipoDeDespacho = new TipoDeDespacho { NombreTipoDespacho = datos[4] },
                            FechaHora = DateTime.Parse(datos[5]),
                            PrecioDia = double.Parse(datos[6]),
                            CantidadDespacho = double.Parse(datos[7]),
                            Total = double.Parse(datos[8])
                        };
                        listaDespacho.InsertarFinal(nuevoDespacho);
                    }
                }
            }
        }



        private void btnIngresarCliente_Click(object sender, EventArgs e)
        {
            comboBoxElegirBomba.Enabled = true;
            btnElegirBomba.Enabled = true;

            MessageBox.Show("Cliente Ingresado");
        }

        private void btnElegirBomba_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Bomba Seleccionada");
            comboBoxEleccion.Enabled = true;
        }

        private void comboBoxEleccion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxEleccion.SelectedItem.ToString() == "Tanque Lleno")
            {
                btnDespacharGasolina.Enabled = true;
                txtCantidadDespachar.Enabled = false;
            }
            else if (comboBoxEleccion.SelectedItem.ToString() == "Prepago")
            {
                btnDespacharGasolina.Enabled = true;
                txtCantidadDespachar.Enabled = true;
            }

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void btnDespacharGasolina_Click(object sender, EventArgs e)
        {
            string nombreCliente = txtNombreCliente.Text;
            string apellidoCliente = TxtApellidoCliente.Text;
            string nitCliente = txtNitCliente.Text;
            string codigoBomba = comboBoxElegirBomba.SelectedItem.ToString();
            string tipoDespacho = comboBoxEleccion.SelectedItem.ToString();
            DateTime fechaHora = DateTime.Now;
            double precioDia = 10.0;
            double cantidadDespacho = double.Parse(txtCantidadDespachar.Text);
            double total = cantidadDespacho * precioDia;

            Cliente cliente = new Cliente { Nombre = nombreCliente, Apellido = apellidoCliente, Nit = nitCliente };
            Bomba bomba = new Bomba { CodigoBomba = codigoBomba };
            TipoDeDespacho tipoDeDespacho = new TipoDeDespacho { NombreTipoDespacho = tipoDespacho };

            Despacho nuevoDespacho = new Despacho
            {
                Cliente = cliente,
                Bomba = bomba,
                TipoDeDespacho = tipoDeDespacho,
                FechaHora = fechaHora,
                PrecioDia = precioDia,
                CantidadDespacho = cantidadDespacho,
                Total = total
            };

            listaDespacho.InsertarFinal(nuevoDespacho);
            listaDespacho.ActualizarArchivoDeTexto();

            // Crear objeto para enviar a Arduino
            DespachoArduino despachoArduino = new DespachoArduino
            {
                TipoDespacho = tipoDespacho,
                CantidadDespacho = cantidadDespacho
            };

            // Serializar a JSON
            string json = JsonSerializer.Serialize(despachoArduino);
            EnviarDatosAArduino(json);

            MessageBox.Show("Despacho registrado y enviado a Arduino correctamente." + json);
        }

        private void EnviarDatosAArduino(string json)
        {
            string puerto = "COM3"; // Reemplaza con el puerto serie correcto
            int baudRate = 9600; // Reemplaza con la velocidad de baudios correcta

            using (SerialPort arduinoPort = new SerialPort(puerto, baudRate))
            {
                arduinoPort.Open();
                arduinoPort.Write(json);
                arduinoPort.Close();
            }
        }


        private void btnGenerarCierreDeCaja_Click(object sender, EventArgs e)
        {
            DateTime fechaSeleccionada = dateTimePicker1.Value;
            List<Despacho> cierreDeCaja = listaDespacho.GeneraradorDeInformes(fechaSeleccionada);


            dataGridViewCierreDeCaja.Rows.Clear();
            foreach (Despacho despacho in cierreDeCaja)
            {
                dataGridViewCierreDeCaja.Rows.Add(
                    despacho.Cliente.Nombre,
                    despacho.Cliente.Apellido,
                    despacho.Cliente.Nit,
                    despacho.Bomba.CodigoBomba,
                    despacho.TipoDeDespacho.NombreTipoDespacho,
                    despacho.FechaHora,
                    despacho.PrecioDia,
                    despacho.CantidadDespacho,
                    despacho.Total);
            }
        }

        private void btnInformeGas_Click(object sender, EventArgs e)
        {
            string tipo = comboBoxAbasteciminento.Text;
            List<Despacho> informeAbastecimiento = listaDespacho.TipoDespacho(tipo);
            dataGridViewInformeAbastecimiento.Rows.Clear();
            foreach (Despacho despacho in informeAbastecimiento)
            {
                dataGridViewInformeAbastecimiento.Rows.Add(
                    despacho.Cliente.Nombre,
                    despacho.Cliente.Apellido,
                    despacho.Cliente.Nit,
                    despacho.Bomba.CodigoBomba,
                    despacho.FechaHora,
                    despacho.PrecioDia,
                    despacho.CantidadDespacho,
                    despacho.Total);
            }

        }

        private void comboBoxAbasteciminento_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxAbasteciminento.SelectedItem.ToString() == "Tanque Lleno" || comboBoxAbasteciminento.SelectedItem.ToString() == "Prepago")
            {
                btnInformeGas.Enabled = true;

            }
        }

        private void comboBoxBombaUsada_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxBombaUsada.SelectedItem.ToString() == "1" || comboBoxBombaUsada.SelectedItem.ToString() == "2")
            {
                btnInformeGas.Enabled = true;

            }

        }
       

        private void DeterminarUsoBomba(string bombaUsada)
        {
            Dictionary<string, int> contadorDespachos = listaDespacho.ContarDespachosPorBomba();

            if (contadorDespachos.Count == 0)
            {
                txtBombaMasUsada.Text = "No hay despachos registrados.";
                return;
            }

            string bombaMasUsada = contadorDespachos.OrderByDescending(b => b.Value).First().Key;
            string bombaMenosUsada = contadorDespachos.OrderBy(b => b.Value).First().Key;

            if (bombaUsada == bombaMasUsada)
            {
                txtBombaMasUsada.Text = "Bomba más usada";
            }
            else if (bombaUsada == bombaMenosUsada)
            {
                txtBombaMasUsada.Text = "Bomba menos usada";
            }
            else
            {
                txtBombaMasUsada.Text = "Bomba de uso intermedio";
            }
        }

        private void btnGenerarBombaUsada_Click(object sender, EventArgs e)
        {
            string bombaUsada = comboBoxBombaUsada.Text;
            List<Despacho> BombaEleccion = listaDespacho.BombaEleccion(bombaUsada);
            dataGridViewBombaUsada.Rows.Clear();
           
            foreach (Despacho despacho in BombaEleccion)
            {
                dataGridViewBombaUsada.Rows.Add(
                    despacho.Cliente.Nombre,
                    despacho.Cliente.Apellido,
                    despacho.Cliente.Nit,
                    despacho.Bomba.CodigoBomba,
                    despacho.FechaHora,
                    despacho.PrecioDia,
                    despacho.CantidadDespacho,
                    despacho.Total);
            }
            DeterminarUsoBomba(bombaUsada);
        }

     
    }
}
