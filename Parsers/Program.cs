using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Parsers
{


    class Program
    {
        static void Main(string[] args)
        {
            Program p = new Program();
            p.Start();

        }


        private string[] elements = { "body", "button", "caption", "div", "h1", "h2", "h3", "h4", "h5", "h6", "label", "table", "th", "td", "tr" };

        #region Metodos
        private string ClearEmailStyle(string body)
        {
            var startIndex = body.IndexOf("<style");
            while (startIndex > -1)
            {
                startIndex = body.IndexOf(">", startIndex) + 1;
                var endIndex = body.IndexOf("</style", startIndex) - 1;
                string estilos = body.Substring(startIndex, endIndex + 1 - startIndex);
                string estilosAlterados = estilos + "";
                estilosAlterados = estilosAlterados.Replace("}", "} #emailBody ");
                estilosAlterados = estilosAlterados.Replace(" * ", "#emailBody");
                estilosAlterados.Insert(0, "#emailBody ");
                body = body.Replace(estilos, estilosAlterados);
                endIndex = body.IndexOf("</style", startIndex) - 1;
                startIndex = body.IndexOf("<style", endIndex);
            }
            return body;
        }
        #endregion

        #region ParseriCarros
        private void ParseriCarros(out string nomeContato, out string telefone, out string email, out string titulo, out decimal preco)
        {
            #region Tratamentos
            string TratarTelefone(string tel)
            {
                tel = tel.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "").Trim();
                if (tel.Length > 14) //Telefone inválido
                    tel = "";
                return tel;
            }
            //Pode ser usado para tratar nome ou título
            string TratarNome(string nomes)
            {
                string pattern = @"(?i)[^0-9a-záéíóúàèìòùâêîôûãõç\s]";
                Regex rgx = new Regex(pattern);
                nomes = rgx.Replace(nomes, "");
                return nomes;
            }
            #endregion

            //GET NOME DO CLIENTE
            nomeContato = "";
            var indiceNome = _email.IndexOf("Nome");
            if (indiceNome >= 0)
            {
                indiceNome = _email.IndexOf("<span", indiceNome);
                if (indiceNome >= 0)
                {
                    indiceNome = _email.IndexOf(">", indiceNome) + 1;
                    var brIndex = _email.IndexOf("</span>", indiceNome);
                    if (indiceNome < _email.Length && brIndex > indiceNome && brIndex - indiceNome < _email.Length)
                        nomeContato = _email.Substring(indiceNome, ((brIndex) - (indiceNome))).Trim();
                    nomeContato = TratarNome(nomeContato);
                }
            }

            //GET TELEFONE
            telefone = "";
            var celular = "";
            var indiceTelefone = _email.IndexOf("Telefone");
            if (indiceTelefone >= 0)
            {
                indiceTelefone = _email.IndexOf("<span", indiceTelefone);
                indiceTelefone = _email.IndexOf(">", indiceTelefone) + 1;
                var endIndiceTelefone = _email.IndexOf("<", indiceTelefone);
                if (indiceTelefone < _email.Length && endIndiceTelefone > indiceTelefone && endIndiceTelefone - indiceTelefone < _email.Length)
                    telefone = _email.Substring(indiceTelefone, endIndiceTelefone - indiceTelefone - 1).Replace("-", String.Empty).Replace(".", String.Empty).Replace("(", String.Empty).Replace(")", String.Empty).Replace(" ", String.Empty).Trim();
                telefone = TratarTelefone(telefone);
            }
            var indiceCelular = _email.IndexOf("Celular");
            if (indiceCelular >= 0)
            {
                indiceCelular = _email.IndexOf("<span", indiceCelular);
                indiceCelular = _email.IndexOf(">", indiceCelular) + 1;
                var endIndiceCelular = _email.IndexOf("<", indiceCelular);
                if (indiceCelular < _email.Length && endIndiceCelular > indiceCelular && endIndiceCelular - indiceCelular < _email.Length)
                    celular = _email.Substring(indiceCelular, endIndiceCelular - indiceCelular - 1).Replace(".", String.Empty).Replace("(", String.Empty).Replace(")", String.Empty).Replace(" ", String.Empty).Replace("-", String.Empty).Trim();
                celular = TratarTelefone(celular);
            }
            if (string.IsNullOrEmpty(telefone))
                telefone = celular;

            //GET EMAIL
            email = "";
            var indiceEmail = _email.IndexOf("Email</span></td>");
            if (indiceEmail >= 0)
            {
                if (indiceNome >= 0)
                {
                    indiceEmail = _email.IndexOf(@";" + "\">", indiceEmail) + 3;
                    var brIndex = _email.IndexOf("</span>", indiceEmail);
                    if (indiceEmail < _email.Length && brIndex > indiceEmail && brIndex - indiceEmail < _email.Length)
                        email = _email.Substring(indiceEmail, brIndex - indiceEmail).Trim();
                }
            }
            else
            {
                indiceEmail = _email.IndexOf("E-mail");
                if (indiceEmail >= 0)
                {
                    indiceEmail = _email.IndexOf("<span", indiceEmail);
                    if (indiceNome >= 0)
                    {
                        indiceEmail = _email.IndexOf(">", indiceEmail) + 1;
                        var brIndex = _email.IndexOf("</span>", indiceEmail);
                        if (indiceEmail < _email.Length && brIndex > indiceEmail && brIndex - indiceEmail < _email.Length)
                            email = _email.Substring(indiceEmail, brIndex - indiceEmail).Trim();
                    }
                }
            }

            //GET Titulo
            titulo = "";
            var indiceTitulo = _email.IndexOf("referente ao an");
            if (indiceTitulo >= 0)
            {
                indiceTitulo = _email.IndexOf("<a", indiceTitulo);
                if (indiceTitulo >= 0)
                {
                    indiceTitulo = _email.IndexOf(">", indiceTitulo) + 1;
                    var aIndex = _email.IndexOf("</a>", indiceTitulo);
                    if (indiceTitulo < _email.Length && aIndex > indiceTitulo && aIndex - indiceTitulo < _email.Length)
                        titulo = _email.Substring(indiceTitulo, aIndex - indiceTitulo).Trim();
                }
            }

            //GET Preço
            preco = 0;
            var indicePreco = titulo.IndexOf("R$") + 2;
            if (indicePreco > 0)
            {
                var splitIndex = titulo.Length;
                if (indicePreco < _email.Length && splitIndex > indicePreco && splitIndex - indicePreco < _email.Length)
                {
                    var _preco = titulo.Substring(indicePreco, splitIndex - indicePreco).Trim();
                    _preco = _preco.Replace(".", ",");
                    string padrao = @"[p][r][e][ç][o][\s]*[R][$]([\d]*[.]?[\d]*)+[,][\d]{2}";
                    Regex rx = new System.Text.RegularExpressions.Regex(padrao);
                    padrao = @"([\d]*[.]?[\d]*)+[,][\d]{2}";
                    rx = new System.Text.RegularExpressions.Regex(padrao);
                    Decimal.TryParse(rx.Match(_preco).Value, out preco);
                    if (preco > 0 && preco < 1000) preco = preco * 1000;
                }
            }
        }
        #endregion


        #region ParserNissan
        private void ParserNissanVentuno(out string nome, out string telefone, out string email, out string titulo)
        {
            /* TRATAMENTOS */
            string TratarTelefone(string tel)
            {
                tel = tel.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "").Trim();
                if (tel.Length > 14) //Telefone inválido
                    tel = "";
                return tel;
            }
            //Pode ser usado para tratar nome ou título
            string TratarNome(string nomes)
            {
                string pattern = @"(?i)[^0-9a-záéíóúàèìòùâêîôûãõç\s]";
                Regex rgx = new Regex(pattern);
                nomes = rgx.Replace(nomes, "");
                return nomes;
            }
            //Obtendo nome
            nome = "";
            var indice = _email.IndexOf("Cliente:</b>");
            if (indice >= 0)
            {
                indice += 12;
                var spnIndex = _email.IndexOf("</p>", indice);
                nome = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
                nome = TratarNome(nome);
            }

            //Obtendo telefone
            telefone = "";
            indice = _email.IndexOf("Contatos:</b>");
            if (indice > 0)
            {
                indice += 13;
                int indexVirgula = _email.IndexOf(",", indice);
                if (indexVirgula > 0)
                {
                    telefone = _email.Substring(indice, (indexVirgula - indice)).Trim();
                    telefone = TratarTelefone(telefone);
                }
                else
                {
                    var spnIndex = _email.IndexOf("</p>", indice) + 4;
                    if (spnIndex > 0)
                        telefone = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
                    telefone = TratarTelefone(telefone);
                }
            }

            //Obtendo e-mail
            email = "";
            indice = _email.IndexOf("E-mail:</b>");
            if (indice > 0)
            {
                indice += 11;
                int indexVirgula = _email.IndexOf(",", indice);
                if (indexVirgula > 0)
                {
                    email = _email.Substring(indice, (indexVirgula - indice)).Trim();
                }
                else
                {
                    var spnIndex = _email.IndexOf("</p>", indice) + 4;
                    if (spnIndex > 0)
                        email = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
                }
            }

            //Obtendo título
            titulo = "";
            indice = _email.IndexOf("interesse:</b>");
            if (indice > 0)
            {
                indice += 14;
                var spnIndex = _email.IndexOf("</p>", indice);
                titulo = _email.Substring(indice, (spnIndex - indice)).Trim();
                titulo = TratarNome(titulo);
            }
        }
        #endregion
        #region ParserSearchOpticsViamondo
        private void ParserSearchOpticsViamondo(out string nomeContato, out string telefone, out string email)
        {
            #region Tratamentos
            string TratarTelefone(string tel)
            {
                tel = tel.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "").Trim();
                if (tel.Length > 14) //Telefone inválido
                    tel = "";
                return tel;
            }
            //Pode ser usado para tratar nome ou título
            string TratarNome(string nomes)
            {
                string pattern = @"(?i)[^0-9a-záéíóúàèìòùâêîôûãõç\s]";
                Regex rgx = new Regex(pattern);
                nomes = rgx.Replace(nomes, "");
                return nomes;
            }
            #endregion

            //GET NOME DO CLIENTE
            nomeContato = "";
            var indice = _email.IndexOf("first_name</td><td>");
            if (indice >= 0)
            {
                indice += 19;
                var spnIndex = _email.IndexOf("</td>", indice);
                nomeContato = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
                nomeContato = TratarNome(nomeContato);
            }

            //GET TELEFONE
            telefone = "";
            indice = _email.IndexOf("mobile</td><td>");
            if (indice > 0)
            {
                indice += 15;
                var spnIndex = _email.IndexOf("</td>", indice);
                telefone = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
                telefone = TratarTelefone(telefone);
            }

            //GET EMAIL
            email = "";
            indice = _email.IndexOf("email</td><td>");
            if (indice > 0)
            {
                indice += 14;
                var spnIndex = _email.IndexOf("</td>", indice);
                email = _email.Substring(indice, ((spnIndex) - (indice))).Trim();

            }
        }
        #endregion
        #region ParserUsadosBR
        private void ParserUsadosBr(out string nomeContato, out string telefone, out string email, out string titulo, out decimal preco)
        {
            /* TRATAMENTOS */
            string TratarTelefone(string tel)
            {
                tel = tel.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "").Trim();
                if (tel.Length > 14) //Telefone inválido
                    tel = "";
                return tel;
            }
            //Pode ser usado para tratar nome ou título
            string TratarNome(string nome)
            {
                string pattern = @"(?i)[^0-9a-záéíóúàèìòùâêîôûãõç\s]";
                Regex rgx = new Regex(pattern);
                nome = rgx.Replace(nome, "");
                return nome;
            }

            //GET NOME DO CLIENTE
            nomeContato = "";
            var indice = _email.IndexOf("Nome:");
            if (indice >= 5)
            {
                indice = _email.IndexOf("</strong>", indice) + 9;
                var spnIndex = _email.IndexOf("<", indice);
                //Pegar de < até >
                var tag = _email.Substring(spnIndex, 4);

                //Proposta
                if (tag.Equals("</p>"))
                {
                    spnIndex = _email.IndexOf("</p>", indice);
                    nomeContato = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
                    nomeContato = TratarNome(nomeContato);
                }
                //Nova simulação
                else if (tag.Equals("<br>"))
                {
                    spnIndex = _email.IndexOf("<br>", indice);
                    nomeContato = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
                    nomeContato = TratarNome(nomeContato);
                }
            }
            else
            {
                //Financiamento e ligação
                //margin:0;"> = 11 caracteres
                indice = _email.IndexOf(@"margin:0" + ";" + "\">") + 11;
                var spnIndex = _email.IndexOf("</p>", indice);
                nomeContato = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
                nomeContato = TratarNome(nomeContato);
            }

            //GET TELEFONE
            telefone = "";
            indice = _email.IndexOf("Telefone:");
            if (indice > 310)
            {
                indice = _email.IndexOf("</strong>", indice) + 9;
                var spnIndex = _email.IndexOf("<", indice);
                //Pegar de < até >
                var tag = _email.Substring(spnIndex, 4);
                if (tag.Equals("<br>"))
                {
                    spnIndex = _email.IndexOf("<br>", indice);
                    telefone = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
                    telefone = TratarTelefone(telefone);
                }
                else
                {
                    indice = _email.IndexOf(@";" + "\">", indice) + 3;
                    spnIndex = _email.IndexOf("</a>", indice);
                    telefone = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
                    telefone = TratarTelefone(telefone);
                }
            }
            else
            {
                //FINANCIAMENTO E LIGAÇÃO
                indice = _email.IndexOf(@"margin:0" + ";" + "\">");
                //Pegar a segunda ocorrência de margin:0;">
                indice = _email.IndexOf(@"margin:0" + ";" + "\">", indice + 1) + 11;
                var spnIndex = _email.IndexOf("</p>", indice);
                telefone = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
                telefone = TratarTelefone(telefone);
            }
            //GET EMAIL
            email = "";
            indice = _email.IndexOf("E-mail:");
            if (indice == -1)
            {
                indice = _email.IndexOf("Email:");
                if (indice >= 0)
                {
                    indice += 15;
                    var spnIndex = _email.IndexOf("<br>", indice);
                    email = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
                }
            }
            else if (indice >= 8)
            {
                indice = _email.IndexOf(@";" + "\">", indice) + 3;
                var spnIndex = _email.IndexOf("</a>", indice);
                email = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
            }
            else if ((_email.IndexOf(nomeContato) + nomeContato.Length + 3) < 400)
            {
                indice = _email.IndexOf(nomeContato) + nomeContato.Length + 3;
                var spnIndex = _email.IndexOf("<br>", indice);
                email = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
            }
            else
            {
                string[] emailValid = email.Split(' ');
                if (string.IsNullOrEmpty(email) || emailValid.Length > 1)
                {
                    indice = _email.IndexOf("Email:</strong>") + 15;
                    if (indice > 15)
                    {
                        var spnIndex = _email.IndexOf("<br>", indice);
                        email = _email.Substring(indice, spnIndex - indice).Trim();
                    }
                }
            }

            //GetTitle
            titulo = "";
            var indiceProduto = _email.IndexOf("Placa:");
            indice = _email.IndexOf("d. ");
            if (indice > 0)
            {
                indice = _email.IndexOf(@";" + "\">", indice) + 37;
                var spnIndex = _email.IndexOf("</p>", indice);
                titulo = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
                titulo = TratarNome(titulo);
            }
            //GetPrice
            preco = 0;
            //indice = _email.IndexOf("d.");
            indice = _email.IndexOf(("R$"), indice) + 2;
            var IndexFimPreco = _email.IndexOf("</p>", indice);
            if (IndexFimPreco > 0)
            {
                var _preco = _email.Substring(indice, (IndexFimPreco - indice)).Trim();
                Decimal.TryParse(_preco, out preco);
            }
        }
        #endregion
        #region ParserMeuCarango
        private void ParserMeuCarango(out string nomeContato, out string telefone, out string email, out string titulo, out decimal preco)
        {
            //GET NOME DO CLIENTE
            nomeContato = "";
            var indice = _email.IndexOf("Nome:") + 4;
            if (indice >= 5)
            {
                indice = _email.IndexOf(">", indice) + 1;
                var spnIndex = _email.IndexOf("<", indice);
                nomeContato = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
            }


            //GET TELEFONE
            telefone = "";
            indice = _email.IndexOf("Tel.:") + 9;
            if (indice >= 10)
            {
                indice = _email.IndexOf(">", indice) + 1;
                var spnIndex = _email.IndexOf("<", indice);
                telefone = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
                telefone = telefone.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "").Trim();
                if (telefone.Length > 14) telefone = ""; //Telefone inválido
            }


            //GET EMAIL
            email = "";
            indice = _email.IndexOf("E-mail:") + 7;
            if (indice >= 8)
            {
                indice = _email.IndexOf(">", indice) + 1;
                var spnIndex = _email.IndexOf("<", indice);
                email = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
            }

            //GetTitle
            titulo = "";
            var indiceProduto = _email.IndexOf("Veículo:");
            if (indiceProduto >= 0)
            {
                indiceProduto = _email.IndexOf(">", indiceProduto) + 1;
                var endIndiceProduto = _email.IndexOf("<", indiceProduto);
                titulo = _email.Substring(indiceProduto, endIndiceProduto - indiceProduto).Trim();
            }

            //GetPrice
            var value = "";
            var indiceValue = _email.IndexOf("Preço:");
            if (indiceValue >= 0)
            {
                indiceValue = _email.IndexOf("R$", indiceValue) + 2;
                var endIndiceValue = _email.IndexOf("<", indiceValue);
                value = _email.Substring(indiceValue, endIndiceValue - indiceValue).Replace('.', ',').Trim();
            }

            Decimal.TryParse(value, out preco);
        }
        #endregion
        #region ParserPortalViasul
        private void ParserPortalViasul(out string nomeContato, out string telefone, out string email, out string titulo, out decimal preco)
        {
            //GET NOME DO CLIENTE
            nomeContato = "";
            var indice = _email.IndexOf("Nome:") + 1;
            if (indice >= 1)
            {
                indice = _email.IndexOf(">", indice) + 1;
                indice = _email.IndexOf(">", indice) + 1;
                var spnIndex = _email.IndexOf("<", indice);
                nomeContato = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
            }


            //GET TELEFONE
            telefone = "";
            indice = _email.IndexOf("Telefone:") + 1;
            if (indice >= 1)
            {
                indice = _email.IndexOf(">", indice) + 1;
                indice = _email.IndexOf(">", indice) + 1;
                var spnIndex = _email.IndexOf("<", indice);
                telefone = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
                telefone = telefone.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "").Replace("\n", "").Replace("\r", "").Trim();
                if (telefone.Length > 14) telefone = ""; //Telefone inválido
            }
            else
            {
                indice = _email.IndexOf("Celular:") + 1;
                if (indice >= 1)
                {
                    indice = _email.IndexOf(">", indice) + 1;
                    indice = _email.IndexOf(">", indice) + 1;
                    var spnIndex = _email.IndexOf("<", indice);
                    telefone = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
                    telefone = telefone.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "").Replace("\n", "").Replace("\r", "").Trim();
                    if (telefone.Length > 14) telefone = ""; //Telefone inválido
                }
            }

            //GET EMAIL
            email = "";
            indice = _email.IndexOf("E-mail:") + 7;
            if (indice >= 8)
            {
                indice = _email.IndexOf(">", indice) + 1;
                indice = _email.IndexOf(">", indice) + 1;
                var spnIndex = _email.IndexOf("<", indice);
                email = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
            }

            //GetTitle
            titulo = "";
            var indiceProduto = _email.IndexOf("Marca:");
            if (indiceProduto >= 0)
            {
                indiceProduto = _email.IndexOf(">", indiceProduto) + 1;
                indiceProduto = _email.IndexOf(">", indiceProduto) + 1;
                var endIndiceProduto = _email.IndexOf("<", indiceProduto);
                titulo = _email.Substring(indiceProduto, endIndiceProduto - indiceProduto).Trim();
                indiceProduto = _email.IndexOf("Modelo:");
                if (indiceProduto >= 0)
                {
                    indiceProduto = _email.IndexOf(">", indiceProduto) + 1;
                    indiceProduto = _email.IndexOf(">", indiceProduto) + 1;
                    endIndiceProduto = _email.IndexOf("<", indiceProduto);
                    titulo = titulo + " " + _email.Substring(indiceProduto, endIndiceProduto - indiceProduto).Trim();

                }
            }

            //GetPrice
            var value = "";
            var indiceValue = _email.IndexOf("R$");
            if (indiceValue >= 0)
            {
                var endIndiceValue = _email.IndexOf("<", indiceValue);
                value = _email.Substring(indiceValue, endIndiceValue - indiceValue).Replace("R", "").Replace("$", "").Trim();
            }

            Decimal.TryParse(value, out preco);
        }
        #endregion
        #region ParserCarango
        private void ParserCarango(out string nomeContato, out string telefone, out string email, out string titulo, out decimal preco)
        {
            //GET NOME DO CLIENTE
            nomeContato = "";
            var indice = _email.IndexOf("Nome:");
            if (indice >= 0)
            {
                indice = _email.IndexOf("Nome:") + 5;
                var spnIndex = _email.IndexOf("<", indice);
                nomeContato = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
            }

            //GET TELEFONE
            telefone = "";
            if (_email.IndexOf("Contato:") >= 0)
            {
                indice = _email.IndexOf("Contato:") + 8;
                var spnIndex = _email.IndexOf("<", indice);
                telefone = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
                telefone = Regex.Replace(telefone, @"\W+", "");
                if (telefone.Length > 14) telefone = ""; //Telefone inválido
            }

            //GET EMAIL
            email = "";
            if (_email.IndexOf("Email:") >= 0)
            {
                indice = _email.IndexOf("Email:") + 6;
                var spnIndex = _email.IndexOf("<", indice);
                email = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
            }

            //GetTitle
            titulo = "";
            var indiceProduto = _email.IndexOf($"height:12px;\">");
            if (indiceProduto >= 0)
            {
                indiceProduto = _email.IndexOf("height:12px;\">", indiceProduto) + 14;
                //indiceProduto = _email.IndexOf(">", indiceProduto + 2);
                var endIndiceProduto = _email.IndexOf("<", indiceProduto);
                titulo = _email.Substring(indiceProduto, endIndiceProduto - indiceProduto).Trim();
            }

            //GetPrice
            var value = "";
            var indiceValue = _email.IndexOf("R$");
            if (indiceValue >= 0)
            {
                var endIndiceValue = _email.IndexOf("<", indiceValue);
                value = _email.Substring(indiceValue, endIndiceValue - indiceValue).Replace("R", "").Replace("$", "").Trim();
            }

            Decimal.TryParse(value, out preco);
        }
        #endregion
        #region ParserCarClick
        private void ParserCarClick(out string nomeContato, out string telefone, out string email, out string titulo, out decimal preco)
        {
            //GET NOME DO CLIENTE
            nomeContato = "";
            var indice = _email.IndexOf("Nome: </strong>");
            if (_email.IndexOf("Nome: </strong>") >= 0)
            {
                indice = _email.IndexOf("Nome: </strong>") + 15;
                var spnIndex = _email.IndexOf("<", indice);
                nomeContato = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
            }

            //GET TELEFONE
            telefone = "";
            if (_email.IndexOf("Telefone: </strong>") >= 0)
            {
                indice = _email.IndexOf("Telefone: </strong>") + 19;
                var spnIndex = _email.IndexOf("<", indice);
                telefone = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
                telefone = Regex.Replace(telefone, @"\W+", "");
                if (telefone.Length > 14) telefone = ""; //Telefone inválido
            }
            else if (_email.IndexOf("Celular: </strong>") >= 0)
            {
                indice = _email.IndexOf("Celular: </strong>") + 18;
                var spnIndex = _email.IndexOf("<", indice);
                telefone = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
                telefone = Regex.Replace(telefone, @"\W+", "");
                if (telefone.Length > 14) telefone = ""; //Telefone inválido
            }

            //GET EMAIL
            email = "";
            if (_email.IndexOf("Email: </strong>") >= 0)
            {
                indice = _email.IndexOf("Email: </strong>") + 16;
                var spnIndex = _email.IndexOf("<", indice);
                email = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
            }

            //GetTitle
            titulo = "";
            var indiceProduto = _email.IndexOf("Modelo: </strong>");
            if (indiceProduto >= 0)
            {
                indiceProduto = _email.IndexOf("Modelo: </strong>", indiceProduto) + 17;
                var endIndiceProduto = _email.IndexOf("<", indiceProduto);
                titulo = _email.Substring(indiceProduto, endIndiceProduto - indiceProduto).Trim();
            }

            //GetPrice
            var value = "";
            var indiceValue = _email.IndexOf("Preço: </strong>");
            if (indiceValue >= 0)
            {
                indiceValue = _email.IndexOf("Preço: </strong>", indiceValue) + 16;
                var endIndiceValue = _email.IndexOf("<", indiceValue);
                value = _email.Substring(indiceValue, endIndiceValue - indiceValue).Replace("R", "").Replace("$", "").Trim();
            }

            Decimal.TryParse(value, out preco);
        }
        #endregion
        #region ParserLarissa
        private void ParserGmail(out string url, out string email)
        {
            //Obtendo url
            url = "";
            //Pegar o índice da frase a partir da sua primeira letra
            var indice = _email.IndexOf(@"URL:");
            if (indice >= 0)
            {
                /* Pegar o índice da primeira ocorrência de ;"> */

                indice = _email.IndexOf(@";" + "\">", indice) + 3;

                /* Pegar o indice de <br> (porque é o que vem depois do nome do cliente) a partir do primeiro indice depois de <strong> */
                var spnIndex = _email.IndexOf("</a>", indice);
                url = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
                //indice = spnIndex + 4;
            }

            //Obtendo email
            email = "";
            var indiceEmail = _email.IndexOf(@"Email:");
            if (indiceEmail >= 0)
            {
                /* Pegar o índice da primeira ocorrência de ;"> */

                indiceEmail = _email.IndexOf(@";" + "\">", indiceEmail) + 3;

                /* Pegar o indice de <br> (porque é o que vem depois do nome do cliente) a partir do primeiro indice depois de <strong> */
                var spnIndex = _email.IndexOf("</a>", indiceEmail);
                email = _email.Substring(indiceEmail, ((spnIndex) - (indiceEmail))).Trim();
                //indice = spnIndex + 4;
            }

        }
        #endregion
        #region ParserAutoSergipe
        private void ParserAutoSergipe(out string nomeContato, out string telefone, out string email, out string titulo)
        {
            //GET NOME DO CLIENTE
            nomeContato = "";
            var indice = _email.IndexOf("Nome:</td>");
            if (_email.IndexOf("Nome:</td>") >= 0)
            {
                indice = _email.IndexOf(">Nome:</td>");
                indice = _email.IndexOf("<td", indice);
                indice = _email.IndexOf(">", indice + 2);
                var endIndiceNome = _email.IndexOf("<", indice);
                nomeContato = _email.Substring(indice + 1, endIndiceNome - indice - 1).Trim();
            }

            //GET TELEFONE
            telefone = "";
            if (_email.IndexOf("Telefone:</td>") >= 0)
            {
                indice = _email.IndexOf("Telefone:</td>");
                indice = _email.IndexOf("<td", indice);
                indice = _email.IndexOf(">", indice + 2);
                var endIndiceTelefone = _email.IndexOf("<", indice);
                telefone = _email.Substring(indice + 1, endIndiceTelefone - indice - 1).Trim();
            }

            //GET EMAIL
            email = "";
            if (_email.IndexOf("E-mail:</td>") >= 0)
            {
                indice = _email.IndexOf("E-mail:</td>");
                indice = _email.IndexOf("<td", indice);
                indice = _email.IndexOf(">", indice + 2);
                var endIndiceEmail = _email.IndexOf("<", indice);
                email = _email.Substring(indice + 1, endIndiceEmail - indice - 1).Trim();
            }

            //GetTitle
            titulo = "";
            if (_email.IndexOf("Título:</td>") >= 0)
            {
                indice = _email.IndexOf("Título:</td>");
                indice = _email.IndexOf("<td", indice);
                indice = _email.IndexOf(">", indice + 2);
                var endIndiceTitle = _email.IndexOf("<", indice);
                titulo = _email.Substring(indice + 1, endIndiceTitle - indice - 1).Trim();
            }

        }
        #endregion
        #region MeuCarroNovo
        private void ParserMeuCarroNovo(out string nome, out string telefone, out string email, out string titulo)
        {
            //Obtendo nome
            nome = "";
            //Pegar o índice da frase a partir da sua primeira letra
            var indice = _email.IndexOf("mensagem do cliente:");
            if (indice >= 0)
            {
                /* Pegar o índice da primeira ocorrência de <strong>, a partir de "mensagem do cliente", e somar 8 (8 caracteres de <strong>) */
                indice = _email.IndexOf("<strong>", indice) + 8;

                /* Pegar o indice de <br> (porque é o que vem depois do nome do cliente) a partir do primeiro indice depois de <strong> */
                var spnIndex = _email.IndexOf("<br>", indice);
                nome = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
                indice = spnIndex + 4;
            }

            //Obtendo telefone
            telefone = "";
            if (_email.IndexOf("cliente:") >= 0)
            {
                var spnIndex = _email.IndexOf("<br>", indice);
                telefone = _email.Substring(indice, ((spnIndex) - (indice))).Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "").Trim();
                if (telefone.Length > 14) telefone = ""; //Telefone inválido
                if (telefone == "&nbsp;") telefone = "";
                indice = spnIndex + 4;
            }

            //Obtendo e-mail
            email = "";
            if (_email.IndexOf("cliente:") >= 0)
            {
                var spnIndex = _email.IndexOf("</strong>", indice);
                email = _email.Substring(indice, ((spnIndex) - (indice))).Trim();
            }

            //Obtendo título
            titulo = "";
            indice = _email.IndexOf("Modelo:") + 7;
            if (_email.IndexOf("Modelo:") >= 7)
            {
                indice = _email.IndexOf("<strong>", indice) + 8;
                var spnIndex = _email.IndexOf("</strong>", indice);
                titulo = _email.Substring(indice, spnIndex - indice).Trim();
            }
        }
        #endregion
        private string ExtrairEmail(string value)
        {
            const string MatchEmailPattern =
           @"(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
           + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
             + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
           + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})";
            Regex re = new Regex(MatchEmailPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            var identifier = re.Match(value);
            if (!string.IsNullOrWhiteSpace(identifier.Value))
                return identifier.Value;

            return string.Empty;
        }

        private string _email;

        private void Start()
        {
            Console.Clear();
            Console.WriteLine("Entre com o caminho do email:");
            string path = Console.ReadLine();

            using (StreamReader sr = new StreamReader(path))
            {
                _email = sr.ReadToEnd();
            }

            Console.WriteLine("\nSelecione o parser:\n");

            foreach (int item in Enum.GetValues(typeof(Parsers)))
            {
                Console.Write($"{item} - {(Parsers)item}\t");

            }

            Console.Write("\n");

            Parsers value = (Parsers)Enum.Parse(typeof(Parsers), Console.ReadLine());

            Console.Write("\n");

            string titulo = null;
            string nomeContato = null;
            string telefone = null;
            string email = null;
            string url = null;
            decimal preco = 0;

            switch (value)
            {
                case Parsers.ICarros:
                    ParseriCarros(out nomeContato, out telefone, out email, out titulo, out preco);
                    break;
                case Parsers.UsadosBr:
                    ParserUsadosBr(out nomeContato, out telefone, out email, out titulo, out preco);
                    break;
                case Parsers.MeuCarango:
                    ParserMeuCarango(out nomeContato, out telefone, out email, out titulo, out preco);
                    break;
                case Parsers.PortalViasul:
                    ParserPortalViasul(out nomeContato, out telefone, out email, out titulo, out preco);
                    break;
                case Parsers.Carango:
                    ParserCarango(out nomeContato, out telefone, out email, out titulo, out preco);
                    break;
                case Parsers.CarClick:
                    ParserCarClick(out nomeContato, out telefone, out email, out titulo, out preco);
                    break;
                case Parsers.AutoSergipe:
                    ParserAutoSergipe(out nomeContato, out telefone, out email, out titulo);
                    break;
                case Parsers.Gmail:
                    ParserGmail(out url, out email);
                    break;
                case Parsers.MeuCarroNovo:
                    ParserMeuCarroNovo(out nomeContato, out telefone, out email, out titulo);
                    break;
                case Parsers.Nissan:
                    ParserNissanVentuno(out nomeContato, out telefone, out email, out titulo);
                    break;
                case Parsers.SearchOpticsViamondo:
                    ParserSearchOpticsViamondo(out nomeContato, out telefone, out email);
                    break;
                default:
                    break;
            }

            Console.WriteLine($"Nome: {nomeContato}");
            Console.WriteLine($"Email: {email}");
            Console.WriteLine($"Telefone: {telefone}");
            Console.WriteLine($"Título: {titulo}");
            Console.WriteLine($"Preço: {preco}");
            //Console.WriteLine($"Url: {url}");

            Console.WriteLine("\nRealizar outra consulta? (0 - Não, 1 - Sim)");
            string repetir = Console.ReadLine();
            while (repetir != "0" && repetir != "1")
                repetir = Console.ReadLine();
            if (repetir == "1")
                Start();
        }
    }

    enum Parsers
    {
        ICarros,
        UsadosBr,
        MeuCarango,
        PortalViasul,
        Carango,
        CarClick,
        AutoSergipe,
        Gmail,
        MeuCarroNovo,
        Nissan,
        SearchOpticsViamondo
    }
}
