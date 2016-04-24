﻿// --------------------------------------------------------------------------------------------------------------------
// FrmPrincipal.cs
//
// FirmaXadesNet - Librería la para generación de firmas XADES
// Copyright (C) 2014 Dpto. de Nuevas Tecnologías de la Concejalía de Urbanismo de Cartagena
//
// This program is free software: you can redistribute it and/or modify
// it under the +terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see http://www.gnu.org/licenses/. 
//
// Contact info: J. Arturo Aguado
// Email: informatica@gemuc.es
// 
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FirmaXadesNet;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.IO;

namespace TestFirmaXades
{
    public partial class FrmPrincipal : Form
    {
        FirmaXades _firmaXades = new FirmaXades();

        public FrmPrincipal()
        {
            InitializeComponent();
        }

        private void FrmPrincipal_Load(object sender, EventArgs e)
        {
            cmbAlgoritmo.SelectedIndex = 0;
        }


        private void btnSeleccionarFichero_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtFichero.Text = openFileDialog1.FileName;
            }
        }

        private void EstablecerPolitica()
        {
            _firmaXades.PolicyIdentifier = txtIdentificadorPolitica.Text;
            _firmaXades.PolicyHash = txtHashPolitica.Text;
            _firmaXades.PolicyUri = txtURIPolitica.Text;
        }

        private TipoAlgoritmo ObtenerAlgoritmo()
        {
            if (cmbAlgoritmo.SelectedIndex == 0)
            {
                return TipoAlgoritmo.SHA1;
            }
            else if (cmbAlgoritmo.SelectedIndex == 1)
            {
                return TipoAlgoritmo.SHA256;
            }
            else
            {
                return TipoAlgoritmo.SHA512;
            }

        }

        private void btnFirmar_Click(object sender, EventArgs e)
        {


            if (string.IsNullOrEmpty(txtFichero.Text))
            {
                MessageBox.Show("Debe seleccionar un fichero para firmar.");
                return;
            }

            if (rbInternnallyDetached.Checked)
            {

                string mimeType = MimeTypeInfo.GetMimeType(txtFichero.Text);

                EstablecerPolitica();

                _firmaXades.InsertarFicheroInternallyDetached(txtFichero.Text, mimeType);
            }
            else if (rbExternallyDetached.Checked)
            {
                _firmaXades.InsertarFicheroExternallyDetached(txtFichero.Text);
            }
            else if (rbEnveloped.Checked)
            {
                _firmaXades.InsertarFicheroEnveloped(txtFichero.Text);
            }

            TipoAlgoritmo tipoAlgoritmo = ObtenerAlgoritmo();

            _firmaXades.AlgoritmoReferencias = tipoAlgoritmo;
            _firmaXades.Firmar(_firmaXades.SeleccionarCertificado(), tipoAlgoritmo);

            MessageBox.Show("Firma completada, ahora puede Guardar la firma o ampliarla a Xades-T.", "Test firma XADES",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private void btnCoFirmar_Click(object sender, EventArgs e)
        {
            EstablecerPolitica();

            TipoAlgoritmo tipoAlgoritmo = ObtenerAlgoritmo();

            _firmaXades.AlgoritmoReferencias = tipoAlgoritmo;
            _firmaXades.CoFirmar(_firmaXades.SeleccionarCertificado(), tipoAlgoritmo);

            MessageBox.Show("Firma completada correctamente.", "Test firma XADES",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnXadesT_Click(object sender, EventArgs e)
        {
            try
            {
                _firmaXades.URLServidorTSA = txtURLSellado.Text;

                _firmaXades.AmpliarAXadesT();

                MessageBox.Show("Sello de tiempo aplicado correctamente.\nAhora puede Guardar la firma o ampliarla a Xades-XL", "Test firma XADES",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ha ocurrido un error ampliando la firma: " + ex.Message);
            }
        }

        private void btnXadesXL_Click(object sender, EventArgs e)
        {
            try
            {
                _firmaXades.URLServidorTSA = txtURLSellado.Text;

                // Se asigna el OCSP por defecto en caso de que el certificado emisor
                // no tenga una URL de validación
                _firmaXades.ServidorOCSP = txtOCSP.Text;

                _firmaXades.AmpliarAXadesXL();

                MessageBox.Show("Firma ampliada correctamente a XADES-XL.", "Test firma XADES",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ha ocurrido un error ampliando la firma: " + ex.Message);
            }
        }

        private void GuardarFirma()
        {
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _firmaXades.GuardarFirma(saveFileDialog1.FileName);

                MessageBox.Show("Firma guardada correctamente.");
            }
        }

        private void btnGuardarFirma_Click(object sender, EventArgs e)
        {
            GuardarFirma();
        }

        private void btnCargarFirma_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (FileStream fs = new FileStream(openFileDialog1.FileName, FileMode.Open))
                {
                    var firmas = FirmaXades.CargarFirma(fs);

                    FrmSeleccionarFirma frm = new FrmSeleccionarFirma(firmas);

                    if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        _firmaXades = frm.FirmaSeleccionada;
                    }
                    else
                    {
                        MessageBox.Show("Debe seleccionar una firma.");
                    }
                }
            }
        }


        private void btnContraFirma_Click(object sender, EventArgs e)
        {
            EstablecerPolitica();

            TipoAlgoritmo tipoAlgoritmo = ObtenerAlgoritmo();

            _firmaXades.AlgoritmoReferencias = tipoAlgoritmo;
            _firmaXades.ContraFirma(_firmaXades.SeleccionarCertificado(), tipoAlgoritmo);

            MessageBox.Show("Firma completada correctamente.", "Test firma XADES",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnFirmarHuella_Click(object sender, EventArgs e)
        {
            if (!rbInternnallyDetached.Checked)
            {
                MessageBox.Show("Por favor, seleccione el tipo de firma internally detached.");
                return;
            }

            if (string.IsNullOrEmpty(txtFichero.Text))
            {
                MessageBox.Show("Debe seleccionar un fichero para firmar.");
                return;
            }

            EstablecerPolitica();

            _firmaXades.InsertarFicheroInternallyDetached(txtFichero.Text, "hash/sha256");

            TipoAlgoritmo tipoAlgoritmo = ObtenerAlgoritmo();

            _firmaXades.AlgoritmoReferencias = tipoAlgoritmo;
            _firmaXades.Firmar(_firmaXades.SeleccionarCertificado(), tipoAlgoritmo);

            MessageBox.Show("Firma completada, ahora puede Guardar la firma o ampliarla a Xades-T.", "Test firma XADES",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

    }
}
