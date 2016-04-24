using FirmaXadesNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoFirmaElemento
{
    public partial class FrmPrincipal : Form
    {
        public FrmPrincipal()
        {
            InitializeComponent();
        }

        private void btnFirmar_Click(object sender, EventArgs e)
        {
            FirmaXades firmaXades = new FirmaXades();
            string ficheroXml = Application.StartupPath + "\\xsdBOE-A-2011-13169_ex_XAdES_Internally_detached.xml";

            string contenidoXml = File.ReadAllText(ficheroXml);

            firmaXades.FirmarElementoInternallyDetached(contenidoXml, "CONTENT-12ef114d-ac6c-4da3-8caf-50379ed13698", "text/xml");

            Dictionary<string, string> namespaces = new Dictionary<string, string>();
            namespaces.Add("enidoc", "http://administracionelectronica.gob.es/ENI/XSD/v1.0/documento-e");
            namespaces.Add("enidocmeta", "http://administracionelectronica.gob.es/ENI/XSD/v1.0/documento-e/metadatos");
            namespaces.Add("enids", "http://administracionelectronica.gob.es/ENI/XSD/v1.0/firma");
            namespaces.Add("enifile", "http://administracionelectronica.gob.es/ENI/XSD/v1.0/documento-e/contenido");

            firmaXades.EstablecerNodoDestinoFirma("enidoc:documento/enids:firmas/enids:firma/enids:ContenidoFirma/enids:FirmaConCertificado", namespaces);

            firmaXades.Firmar(firmaXades.SeleccionarCertificado());

            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (FileStream fs = new FileStream(saveFileDialog1.FileName, FileMode.Create))
                {
                    firmaXades.GuardarFirma(fs);
                }

                MessageBox.Show("Fichero guardado correctamente.");
            }
        }
    }
}
