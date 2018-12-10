using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Estatisticas
{
    public class Program
    {
        private static string DADOS = System.Environment.CurrentDirectory + "\\D_QUINA.txt";

        private static readonly int[] quadrante1 = new int[] { 1, 2, 3, 4, 5, 11, 12, 13, 14, 15, 21, 22, 23, 24, 25, 31, 32, 33, 34, 35 };
        private static readonly int[] quadrante2 = new int[] { 6, 7, 8, 9, 10, 16, 17, 18, 19, 20, 26, 27, 28, 29, 30, 36, 37, 38, 39, 40 };
        private static readonly int[] quadrante3 = new int[] { 41, 42, 43, 44, 45, 51, 52, 53, 54, 55, 61, 62, 63, 64, 65, 71, 72, 73, 74, 75 };
        private static readonly int[] quadrante4 = new int[] { 46, 47, 48, 49, 50, 56, 57, 58, 59, 60, 66, 67, 68, 69, 70, 76, 77, 78, 79, 80 };
        private static readonly string[] legendas = new string[] { "Nada", "Um", "* Duque", "* Terno", "* Quadra", "* Quina" };

        private static IList<int[]> meusJogos;
        private static string arqIn;
        private static string arqOut;
        private static string arqOutJogos;
        private static string arqInJogos;
        private static string arqResultIn;
        private static bool ordenaArquivo;
        private static bool zerado;
        private static string nomeArqInteiro;
        private static int totalPartes;
        private static bool cabecalho;

        private static void leLinhas()
        {
            Console.Write("Contando linhas do arquivo...");
            int totalLinhas = File.ReadLines(nomeArqInteiro).Count();
            Console.WriteLine($" { String.Format(CultureInfo.InvariantCulture, "{0:0,0}", totalLinhas) } - OK\nFragmentando o arquivo...");
            string linhaCabecalho = string.Empty;
            int atual = 0;
            if (cabecalho)
            {
                linhaCabecalho = File.ReadLines(nomeArqInteiro).Take(1).First();
                totalLinhas--;
                atual = 1;
            }
            int linhasBloco = totalLinhas / totalPartes;
            int resto = totalLinhas % totalPartes;
            int parteAtual = 1;
            while (atual < totalLinhas)
            {
                Console.Write($"Gerando parte { parteAtual }...");
                List<string> linhas = File.ReadLines(nomeArqInteiro).Skip(atual).Take(linhasBloco).ToList();
                Console.Write($" { String.Format(CultureInfo.InvariantCulture, "{0:0,0}", linhas.Count) } linhas { (cabecalho ? " + 1 cabeçalho" : "")} ");
                if (cabecalho)
                {
                    linhas.Insert(0, $"{ linhaCabecalho }");
                }
                File.WriteAllLines($"{ nomeArqInteiro }.parte{ parteAtual++ }.csv", linhas);
                Console.WriteLine(" OK");
                atual += linhasBloco;
            }
            Console.WriteLine("Fragmentação terminada.");
        }

        private static void geraTodosNumeros()
        {
            Stopwatch tempoTotal = new Stopwatch();
            tempoTotal.Start();
            HashSet<string> grupoHash = new HashSet<string>();
            for (int x = 1; x <= 76; x++)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Console.Write($"{ x }: ");
                for (int y = 2; y <= 77; y++)
                {
                    for (int z = 3; z <= 78; z++)
                    {
                        for (int a = 4; a <= 79; a++)
                        {
                            for (int b = 5; b <= 80; b++)
                            {
                                int[] vetor = new int[] { x, y, z, a, b };
                                if (vetor.Length - vetor.ToList().Distinct().Count() == 0)
                                {
                                    Array.Sort(vetor);
                                    grupoHash.Add($"{ vetor[0] }-{ vetor[1] }-{ vetor[2] }-{ vetor[3] }-{ vetor[4] }");
                                }
                            }
                        }
                    }
                }
                sw.Stop();
                double tempoFim = (double)sw.ElapsedTicks / Stopwatch.Frequency;
                Console.WriteLine($"{ String.Format(CultureInfo.InvariantCulture, "{0:0,0}", grupoHash.Count) }/24,040,016 - { string.Format("{0:##0.0000000000}", tempoFim).PadRight(14) }");
            }
            tempoTotal.Stop();
            double tempoTotalFim = (double)tempoTotal.ElapsedTicks / Stopwatch.Frequency;
            Console.WriteLine($"Total de combinações geradas: { String.Format(CultureInfo.InvariantCulture, "{0:0,0}", grupoHash.Count) } - Tempo total: { string.Format("{0:####0.0000000000}", tempoTotalFim) }");
            Console.Write("Gravando arquivo com todos os jogos...");
            using (System.IO.StreamWriter arqSaida = new System.IO.StreamWriter(arqOutJogos))
            {
                foreach (string vetor in grupoHash)
                {
                    arqSaida.WriteLine($"{ vetor }");
                }
            }
            Console.WriteLine(" OK");
            if (zerado)
            {
                Console.Write("Gravando arquivo com todos os jogos contadores zerados...");
                using (System.IO.StreamWriter arqSaida = new System.IO.StreamWriter($"{ arqOutJogos }.zerado.csv"))
                {
                    arqSaida.WriteLine($"jogo;{ legendas[0] };{ legendas[1] };{ legendas[2] };{ legendas[3] };{ legendas[4] };{ legendas[5] }");
                    foreach (string vetor in grupoHash)
                    {
                        arqSaida.WriteLine($"{ vetor };0;0;0;0;0;0");
                    }
                }
                Console.WriteLine(" OK");
            }
        }

        private static void preparaParametros(string[] args)
        {
            var regexArqIn = new Regex(@"^[-/](?i:ae):");
            var regexArqOut = new Regex(@"^[-/](?i:as):");
            var regexJogos = new Regex(@"^[-/](?i:jo):(?i:[an])");
            var regexAjuda = new Regex(@"^[-/][?]");
            var regexGeraArquivoJogo = new Regex(@"^[-/](?i:gaj):");
            var regexTodosJogosZerado = new Regex(@"^[-/](?i:z)");
            var regexArqResultIn = new Regex(@"^[-/](?i:are):");
            var regexOrdenaResultado = new Regex(@"^[-/](?i:o)");
            //var regexParticionaArquivo = new Regex(@"^[-/]");
            StringBuilder parametros = new StringBuilder("");
            foreach (string argumento in args)
            {
                parametros.Append(argumento.Replace(" ", ""));
            }
            string linhaComando = parametros.ToString();
            if (regexAjuda.IsMatch(linhaComando))
            {
                Console.WriteLine("Verifica jogos da quina.");
                Console.WriteLine("ESTATISTICAS [/GAJ:[nome_arquivo_todos_jogos]] [/AE:nome_arquivo_entrada] [/AS:nome_arquivo_saida]\n  [/JO:N1, 2, 3, 4, 5; 8, 9, 10, 11, 12] [/JO:Anome_arquivo_jogos] [/ARE:nome_arquivo_resultados] [/O] [/Z]");
                Console.WriteLine("ESTATISTICAS [/?]");
                Console.WriteLine("ESTATISTICAS\n");
                Console.WriteLine("  /AE:nome_arquivo_entrada\n\t\tEspecifica o nome do arquivo de entrada com dados a serem processados.\n\t\tSe omitido, usa arquivo padrão \"D_QUINA.txt\" da pasta atual.");
                Console.WriteLine("  /AS:nome_arquivo_saida\n\t\tEspecifica o nome do arquivo de saída do processamento.\n\t\tSe omitido, o resultado é impresso na tela.");
                Console.WriteLine("  /GAJ:[nome_arquivo_todos_jogos]\n\t\tEspecifica que será gerado o arquivo com todas as combinações de jogos.\n\t\tSe o nome do arquivo for omitido, é assumido \"todosJogos.txt\" na pasta corrente.");
                Console.WriteLine("  /JO:N1, 2, 3, 4, 5\n  /JO:N1, 2, 3, 4, 5; 10, 20, 30, 40, 50\n\t\tOpção N especifica os possíveis jogos a serem testados no universo de sorteios.\n\t\tCada conjunto de jogos (5 dezenas) é separado por vírgula.\n\t\tCaso mais jogos devam ser testados, cada bloco deve ser separado por ponto e vírgula.");
                Console.WriteLine("  /JO:Anome_arquivo_jogos\n\t\tOpção A especifica o nome do arquivo que contém os jogos a serem testados.");
                Console.WriteLine("  /ARE:nome_arquivo_resultados\n\t\tEspecifica que o arquivo com resultados já processados terá as estatísticas\n\t\tatualizadas em função do novo jogo/conjunto de jogos.");
                Console.WriteLine("  /O\n\t\tOrdena o arquivo de entrada. Criando uma cópia com o mesmo nome porém, csv.");
                Console.WriteLine("  /Z\n\t\tInicializa os contadores do arquivo de todos os jogos com 0 (zero).");
                Console.WriteLine("  /?\n\t\tMostra essa ajuda. Deve ser o primeiro parâmetro.");
                Console.WriteLine("  \t\tSem parâmetros, processa dados padrão com saída na tela.");
                Console.WriteLine("\nExemplos:\n\tEstatisticas /?\n\tEstatisticas /AE:D_QUINA.txt /JO:AtodosJogos.txt /AS:resultado.txt\n\tEstatisticas /AE:D_QUINA.txt /JO:N1, 2, 3, 4, 5; 9, 10, 11, 12, 14\n\tEstatisticas /ARE:resultado.cvs /JO:N1, 2, 3, 4, 5; 9, 10, 11, 12, 14\n\tEstatisticas /AE:D_QUINA.txt /O\n\tEstatisticas /GAJ:todosJogos.txt /Z");
                Environment.Exit(0);
            }
            string[] dados = parametros.ToString().Split(new string[] { "-", "/" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string dado in dados)
            {
                string valor = $"-{ dado }";
                if (regexOrdenaResultado.IsMatch(valor))
                {
                    ordenaArquivo = true;
                }
                if (regexArqIn.IsMatch(valor))
                {
                    arqIn = dado.Substring(3);
                }
                if (regexTodosJogosZerado.IsMatch(valor))
                {
                    zerado = true;
                }
                if (regexArqResultIn.IsMatch(valor))
                {
                    arqResultIn = dado.Substring(4);
                }
                if (regexArqOut.IsMatch(valor))
                {
                    arqOut = dado.Substring(3);
                }
                if (regexGeraArquivoJogo.IsMatch(valor))
                {
                    arqOutJogos = dado.Substring(4);
                    arqOutJogos = string.IsNullOrEmpty(arqOutJogos) ? "todosJogos.txt" : arqOutJogos;
                }
                if (regexJogos.IsMatch(valor))
                {
                    if (Char.ToUpper(valor[4]) == 'N')
                    {
                        foreach (string jogo in dado.Substring(4).Split(';'))
                        {
                            meusJogos.Add(Array.ConvertAll(jogo.Split(','), int.Parse));
                        }
                    }
                    else
                    {
                        arqInJogos = dado.Substring(4);
                    }
                }
            }
        }

        public static void Main(string[] args)
        {
            meusJogos = new List<int[]>();
            ordenaArquivo = false;
            arqIn = string.Empty;
            arqOut = string.Empty;
            zerado = false;
            cabecalho = false;
            preparaParametros(args);

            Console.WriteLine();
            if (!string.IsNullOrEmpty(arqOutJogos))
            {
                Console.WriteLine($"Gerando arquivo de combinação com todos os jogos: { arqOutJogos }...");
                geraTodosNumeros();
            }
            else
            {
                if (!string.IsNullOrEmpty(arqInJogos))
                {
                    Console.Write($"Lendo arquivo com jogos: { arqInJogos }...");
                    string[] dadosArq = System.IO.File.ReadAllLines(arqInJogos);
                    Console.Write(" OK\nProcessando dados do arquivo...");
                    foreach (string jogo in dadosArq)
                    {
                        meusJogos.Add(Array.ConvertAll(jogo.Split('-'), int.Parse));
                    }
                    Console.WriteLine(" OK");
                }
                if (!string.IsNullOrEmpty(arqResultIn))
                {
                    atualizaArquivoEstatistica();
                }
                else
                {
                    IList<Jogo> jogos = new List<Jogo>();
                    try
                    {
                        arqIn = (string.IsNullOrEmpty(arqIn) ? DADOS : arqIn);
                        Console.Write($"Lendo arquivo de resultados: { arqIn }...");
                        string[] dadosArq = System.IO.File.ReadAllLines(arqIn);
                        foreach (string linha in dadosArq)
                        {
                            string[] dadosLinha = linha.Split((';'));
                            jogos.Add(new Jogo(dadosLinha[0],
                                               dadosLinha[1],
                                               new string[] { dadosLinha[2], dadosLinha[3], dadosLinha[4], dadosLinha[5], dadosLinha[6] }));
                        }
                        Console.WriteLine(" OK");
                        if (ordenaArquivo)
                        {
                            Console.Write($"Gravando arquivo com jogos ordenados...");
                            using (System.IO.StreamWriter arqOrdenado = new System.IO.StreamWriter($"{ arqIn }.csv"))
                            {
                                foreach (Jogo jogo in jogos)
                                {
                                    arqOrdenado.WriteLine($"{ jogo.ToString() }");
                                }
                                Console.WriteLine(" OK");
                            }
                        }
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine($"\nFalha ao ler arquivo...\nPrograma será terminado.\n{ e.Message }");
                        Environment.Exit(0);
                    }
                    StringBuilder resultado = new StringBuilder(geraEstatisticaNumeros(jogos));
                    resultado.Append(meusJogos.Count > 0 ? verificaMeusJogos(ref meusJogos, jogos) : "\nNão há jogos para testar!");
                    if (string.IsNullOrEmpty(arqOut))
                    {
                        Console.WriteLine(resultado.ToString());
                        Console.WriteLine("Pressione uma tecla para finalizar...");
                        Console.ReadKey();
                    }
                    else
                    {
                        System.IO.File.WriteAllText(arqOut, resultado.ToString());
                    }
                }
            }
            Console.WriteLine("Finalizado!");
        }

        private static void atualizaArquivoEstatistica()
        {
            IList<Resultado> resultados = new List<Resultado>();
            Console.Write($"Lendo arquivo de estatísticas: { arqResultIn }...");
            string[] dadosArq = System.IO.File.ReadAllLines(arqResultIn);
            Console.WriteLine($" OK");
            Console.Write($"Processando dados...");
            for (int dado = 1; dado < dadosArq.Length; dado++)
            {
                resultados.Add(new Resultado(dadosArq[dado]));
                dadosArq[dado] = "";
            }
            dadosArq = new string[] { "" };
            Console.Write($" OK\nAtualizando dados estatísticos...");
            int contaJogo = 1;
            Console.WriteLine($"Jogo:");
            Stopwatch sw = new Stopwatch();
            int pulaLinha = 0;
            foreach (int[] meuJogo in meusJogos)
            {
                sw.Start();
                Console.Write($"{ string.Format("{0,8}", contaJogo++) }/{ string.Format("{0,4}", meusJogos.Count).PadRight(4) }");
                pulaLinha++;
                int[] total = new int[6];
                foreach (Resultado jogo in resultados)
                {
                    int conta = 0;
                    foreach (int dezena in meuJogo)
                    {
                        conta += jogo.DezenasJogo.Contains(dezena) ? 1 : 0;
                    }
                    jogo.AtualizaEstatistica(conta);
                }
                double tempoFim = (double)sw.ElapsedTicks / Stopwatch.Frequency;
                Console.Write($" - { string.Format("{0:##0.0000000000}", tempoFim).PadRight(14) }");
                sw.Restart();
                if (pulaLinha == 3)
                {
                    Console.WriteLine("");
                    pulaLinha = 0;
                }
            }
            Console.Write($"\nGravando arquivo atualizado...");
            using (System.IO.StreamWriter arqSaida = new System.IO.StreamWriter($"novo_{ arqResultIn }"))
            {
                arqSaida.WriteLine($"jogo;{ legendas[0] };{ legendas[1] };{ legendas[2] };{ legendas[3] };{ legendas[4] };{ legendas[5] }");
                foreach (Resultado resultado in resultados)
                {
                    arqSaida.WriteLine($"{ resultado.ToString() }");
                }
            }
            Console.WriteLine(" OK");
        }

        private static string verificaMeusJogos(ref IList<int[]> meusJogos, IList<Jogo> jogos)
        {
            StringBuilder resultado = new StringBuilder("");
            int contaPar = 0;
            if (string.IsNullOrEmpty(arqInJogos))
            {
                int contaJogo = 1;
                foreach (int[] meuJogo in meusJogos)
                {
                    resultado.Append($"\nJogo { contaJogo++ }{ formataNumeroJogo(meuJogo.ToList()) }");
                    int[] total = new int[6];
                    foreach (Jogo jogo in jogos)
                    {
                        int conta = 0;
                        contaPar = 0;
                        foreach (int dezena in meuJogo)
                        {
                            conta += jogo.DezenasJogo.Contains(dezena) ? 1 : 0;
                            contaPar += dezena % 2 == 0 ? 1 : 0;
                        }
                        total[conta]++;
                    }
                    for (int acerto = 0; acerto <= 5; acerto++)
                    {
                        resultado.Append($"{ legendas[acerto].PadLeft(8) } - { string.Format("{0,4}", total[acerto]) } - { Math.Round((double)total[acerto] / jogos.Count * 100, 2) }%\n");
                    }
                    resultado.Append($"Quadrantes: { montaLegendaQuadrante(posicaoQuadrante(verificaQuadranteDezenas(meuJogo))) }\n");
                    resultado.Append($"Pares: { contaPar }\n");
                }
                Console.WriteLine();
            }
            else
            {
                double tempoFim = 0;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                int totalPassos = meusJogos.Count / 5000;
                Console.WriteLine($"\nGerando arquivo parcial { arqOut }.csv. Total de passos: { totalPassos } + { (meusJogos.Count % 5000 != 0 ? 1 : 0) } parcial - { String.Format(CultureInfo.InvariantCulture, "{0:0,0}", meusJogos.Count) }.");
                int passo = 1;
                resultado.Append($"jogo;{ legendas[0] };{ legendas[1] };{ legendas[2] };{ legendas[3] };{ legendas[4] };{ legendas[5] };Quadrantes;Pares");
                int pacote = 0;
                StreamWriter saida = null;
                while (meusJogos.Count > 0)
                {
                    int[] meuJogo = meusJogos[0];
                    resultado.Append($"\n{ meuJogo[0] }-{ meuJogo[1] }-{ meuJogo[2] }-{ meuJogo[3] }-{ meuJogo[4] };");
                    int[] total = new int[6];
                    foreach (Jogo jogo in jogos)
                    {
                        int conta = 0;
                        contaPar = 0;
                        foreach (int dezena in meuJogo)
                        {
                            conta += jogo.DezenasJogo.Contains(dezena) ? 1 : 0;
                            contaPar += dezena % 2 == 0 ? 1 : 0;
                        }
                        total[conta]++;
                    }
                    resultado.Append($"{ total[0] };{ total[1] };{ total[2] };{ total[3] };{ total[4] };{ total[5] };{ montaLegendaQuadrante(posicaoQuadrante(verificaQuadranteDezenas(meuJogo))) };{ contaPar }");
                    meusJogos.RemoveAt(0);
                    pacote++;
                    if (pacote >= 5000)
                    {
                        saida = new StreamWriter((string.IsNullOrEmpty(arqOut) ? "result" : arqOut) + ".csv", true);
                        using (saida)
                        {
                            saida.WriteLine(resultado.ToString());
                        }
                        resultado.Clear();
                        tempoFim = (double)sw.ElapsedTicks / Stopwatch.Frequency;
                        Console.WriteLine($"\tPasso { passo++} / { totalPassos } - { String.Format(CultureInfo.InvariantCulture, "{0:0,0}", meusJogos.Count) } - OK - { string.Format("{0:##0.0000000000}", tempoFim) }");
                        sw.Restart();
                        pacote = 0;
                    }
                }
                tempoFim = (double)sw.ElapsedTicks / Stopwatch.Frequency;
                Console.WriteLine($"\tPasso parcial - { String.Format(CultureInfo.InvariantCulture, "{0:0,0}", meusJogos.Count) } - OK - { string.Format("{0:##0.0000000000}", tempoFim) }");
                Console.Write($"Gravando arquivo final: { arqOut }.csv...");
                saida = new StreamWriter((string.IsNullOrEmpty(arqOut) ? "result" : arqOut) + ".csv", true);
                using (saida)
                {
                    saida.Write(resultado.ToString());
                }
                resultado.Clear();
                Console.WriteLine(" OK");
            }
            return resultado.ToString();
        }

        private static int[] verificaQuadranteDezenas(int[] dezenas)
        {
            int[] quadrantes = new[] { 0, 0, 0, 0 };
            foreach (int dezena in dezenas)
            {
                quadrantes[verificaQuadrante(dezena)]++;
            }
            return quadrantes;
        }
        private static string geraEstatisticaNumeros(IList<Jogo> jogos)
        {
            Console.Write("Iniciando cálculos estatísticos...");
            int[] chaves = Enumerable.Range(0, 80).ToArray();
            int[] jogosQuadrantes = new int[15];
            int[] totalNumerosQuadrante = new int[4] { 0, 0, 0, 0 };
            int[] contadorNumeros = new int[80];
            int totalDezenas = jogos.Count * 5;
            IList<int> umaDezenaPar = new List<int>();
            IList<int> duasDezenasPares = new List<int>();
            IList<int> tresDezenasPares = new List<int>();
            IList<int> quatroDezenasPares = new List<int>();
            IList<int> soDezenasPares = new List<int>();
            IList<int> soDezenasImpares = new List<int>();
            int menorDezenaFinal = 80;
            int maiorDezenaInicial = 1;
            IList<Jogo> jogosMenorNumero = new List<Jogo>();
            IList<Jogo> jogosMaiorNumero = new List<Jogo>();
            foreach (Jogo jogo in jogos)
            {
                if (jogo.DezenasJogo[4] < menorDezenaFinal)
                {
                    menorDezenaFinal = jogo.DezenasJogo[4];
                    jogosMenorNumero.Clear();
                    jogosMenorNumero.Add(jogo);
                }
                else
                {
                    if (jogo.DezenasJogo[4] == menorDezenaFinal)
                    {
                        jogosMenorNumero.Add(jogo);
                    }
                }
                if (jogo.DezenasJogo[0] > maiorDezenaInicial)
                {
                    maiorDezenaInicial = jogo.DezenasJogo[0];
                    jogosMaiorNumero.Clear();
                    jogosMaiorNumero.Add(jogo);
                }
                else
                {
                    if (jogo.DezenasJogo[0] == maiorDezenaInicial)
                    {
                        jogosMaiorNumero.Add(jogo);
                    }
                }
                int contaPar = 0;
                foreach (int dezena in jogo.DezenasJogo)
                {
                    contadorNumeros[dezena - 1]++;
                    contaPar += dezena % 2 == 0 ? 1 : 0;
                    totalNumerosQuadrante[verificaQuadrante(dezena)]++;
                }
                jogosQuadrantes[posicaoQuadrante(verificaQuadranteDezenas(jogo.DezenasJogo)) - 1]++;
                switch (contaPar)
                {
                    case 0:
                        soDezenasImpares.Add(jogo.NumJogo);
                        break;
                    case 1:
                        umaDezenaPar.Add(jogo.NumJogo);
                        break;
                    case 2:
                        duasDezenasPares.Add(jogo.NumJogo);
                        break;
                    case 3:
                        tresDezenasPares.Add(jogo.NumJogo);
                        break;
                    case 4:
                        quatroDezenasPares.Add(jogo.NumJogo);
                        break;
                    default:
                        soDezenasPares.Add(jogo.NumJogo);
                        break;
                }
            }
            int pares = 0;
            for (int dezena = 0; dezena < contadorNumeros.Count(); dezena++)
            {
                pares += dezena % 2 == 0 ? 0 : contadorNumeros[dezena];
            }
            StringBuilder resultado = new StringBuilder($"Jogos processados: { jogos.Count }.\n");
            resultado.Append($"Período entre { jogos[0].DataJogo.ToShortDateString() } e { jogos[jogos.Count - 1].DataJogo.ToShortDateString() }.");
            resultado.Append("\nOrdem: Dezena");
            resultado.Append(converteVetorImpressao(chaves, contadorNumeros));
            var paresOrdenados = contadorNumeros.Select((x, i) => new { Value = x, Key = chaves[i] })
                        .OrderBy(x => x.Value)
                        .ThenBy(x => x.Key)
                        .ToArray();
            contadorNumeros = paresOrdenados.Select(x => x.Value).ToArray();
            chaves = paresOrdenados.Select(x => x.Key).ToArray();
            resultado.Append("\n\nOrdem: Menor para a Maior Dezena");
            resultado.Append(converteVetorImpressao(chaves, contadorNumeros));
            resultado.Append(numerosMenosSorteados(chaves, contadorNumeros));
            resultado.Append(numerosMedioSorteados(chaves, contadorNumeros));
            resultado.Append(numerosMaisSorteados(chaves, contadorNumeros));
            resultado.Append($"\nTotal dezenas: { totalDezenas }");
            resultado.Append($"\nTotal dezenas pares: { pares } - { Math.Round((double)pares / totalDezenas * 100, 2) }%");
            resultado.Append($"\nTotal dezenas ímpares: { (totalDezenas - pares) } - { Math.Round((double)(totalDezenas - pares) / totalDezenas * 100, 2) }%\n");
            resultado.Append($"\nJogos só com dezenas pares: { soDezenasPares.Count } - { Math.Round((double)soDezenasPares.Count / jogos.Count * 100, 2) }%");
            resultado.Append(formataNumeroJogo(soDezenasPares));
            resultado.Append($"\nJogos só com dezenas ímpares: { soDezenasImpares.Count } - { Math.Round((double)soDezenasImpares.Count / jogos.Count * 100, 2) }%");
            resultado.Append(formataNumeroJogo(soDezenasImpares));
            resultado.Append($"\nJogos com 1 dezena par e 4 ímpares: { umaDezenaPar.Count } - { Math.Round((double)umaDezenaPar.Count / jogos.Count * 100, 2) }%");
            resultado.Append(formataNumeroJogo(umaDezenaPar));
            resultado.Append($"\nJogos com 2 dezenas pares e 3 ímpares: { duasDezenasPares.Count } - { Math.Round((double)duasDezenasPares.Count / jogos.Count * 100, 2) }%");
            resultado.Append(formataNumeroJogo(duasDezenasPares));
            resultado.Append($"\nJogos com 3 dezenas pares e 2 ímpares: { tresDezenasPares.Count } - { Math.Round((double)tresDezenasPares.Count / jogos.Count * 100, 2) }%");
            resultado.Append(formataNumeroJogo(tresDezenasPares));
            resultado.Append($"\nJogos com 4 dezenas pares e 1 ímpar: { quatroDezenasPares.Count } - { Math.Round((double)quatroDezenasPares.Count / jogos.Count * 100, 2) }%");
            resultado.Append(formataNumeroJogo(quatroDezenasPares));
            resultado.Append($"\nMenor dezena final: { menorDezenaFinal } - Total: { jogosMenorNumero.Count }");
            resultado.Append(formataJogo(jogosMenorNumero));
            resultado.Append($"\nMaior dezena inicial: { maiorDezenaInicial } - Total: { jogosMaiorNumero.Count }");
            resultado.Append(formataJogo(jogosMaiorNumero));
            resultado.Append(mostraJogosQuadrante(jogosQuadrantes, jogos.Count));
            for (int quadrante = 0; quadrante < totalNumerosQuadrante.Length; quadrante++)
            {
                resultado.Append($"Total Q{ quadrante + 1 }: { totalNumerosQuadrante[quadrante] } - {  Math.Round((double)totalNumerosQuadrante[quadrante] / totalDezenas * 100, 2) }%\n");
            }
            return resultado.ToString();
        }

        private static int posicaoQuadrante(int[] quadrantes)
        {
            String valorLogico = "";
            foreach (int x in quadrantes)
            {
                valorLogico = (x > 0 ? "1" : "0") + valorLogico;
            }
            return Convert.ToInt32(valorLogico, 2);
        }

        private static string imprimeQuadrantes(int quadrante, int[] dados)
        {
            StringBuilder resultado = new StringBuilder($"Quadrante: { quadrante } -");
            foreach (int dezena in dados)
            {
                resultado.Append($"{ string.Format("{0,3}", dezena) } ");
            }
            resultado.Append("\n");
            return resultado.ToString();
        }

        private static string mostraJogosQuadrante(int[] jogosQuadrantes, int totalJogos)
        {
            StringBuilder resultado = new StringBuilder("\nJogos classificados por quadrantes:\n");
            resultado.Append(imprimeQuadrantes(1, quadrante1));
            resultado.Append(imprimeQuadrantes(2, quadrante2));
            resultado.Append(imprimeQuadrantes(3, quadrante3));
            resultado.Append(imprimeQuadrantes(4, quadrante4));
            Dictionary<string, int> quadrantes = new Dictionary<string, int>();
            for (int quadrante = 1; quadrante <= 15; quadrante++)
            {
                quadrantes.Add(montaLegendaQuadrante(quadrante), jogosQuadrantes[quadrante - 1]);
            }
            List<string> chaves = quadrantes.Keys.ToList();
            chaves.Sort();
            foreach (string chave in chaves)
            {
                resultado.Append($"{ chave.PadLeft(8) }: ");
                resultado.Append($"{ string.Format("{0,4}", quadrantes[chave]) } - { Math.Round((double)quadrantes[chave] / totalJogos * 100, 2) }%\n");
            }
            return resultado.ToString();
        }

        private static string montaLegendaQuadrante(int quadrante)
        {
            string legenda = "";
            string binario = new string(Convert.ToString(quadrante, 2).PadLeft(4, '0').Reverse().ToArray());
            for (int posicao = 3; posicao >= 0; posicao--)
            {
                legenda = (binario[posicao] == '1' ? $"Q{ posicao + 1 }" : "") + legenda;
            }
            return legenda;
        }

        private static string formataJogo(IList<Jogo> jogos)
        {
            StringBuilder resultado = new StringBuilder("\n");
            foreach (Jogo jogo in jogos)
            {
                resultado.Append($"{ string.Format("{0,5}", jogo.NumJogo) }:");
                foreach (int dezena in jogo.DezenasJogo)
                {
                    resultado.Append($"{ string.Format("{0,3}", dezena) } ");
                }
                resultado.Append("\n");
            }
            return resultado.ToString();
        }

        private static int verificaQuadrante(int dezena)
        {
            return quadrante1.Contains(dezena) ? 0 :
                   quadrante2.Contains(dezena) ? 1 :
                   quadrante3.Contains(dezena) ? 2 : 3;
        }

        private static string formataNumeroJogo(IList<int> jogos)
        {
            StringBuilder resposta = new StringBuilder("\n");
            int conta = -1;
            foreach (int jogo in jogos)
            {
                conta += conta == 16 ? -16 : 1;
                resposta.Append($"{ string.Format("{0,5}", jogo) }");
                resposta.Append(conta == 16 ? "\n" : "  ");
            }
            resposta.Append("\n");
            return resposta.ToString();
        }

        private static string montaNumero(int posicao, int[] chaves, int[] dados)
        {
            return $"{ string.Format("{0,2}", chaves[posicao] + 1) }: { string.Format("{0,3}", dados[posicao]) }\n";
        }

        private static string numerosMedioSorteados(int[] chaves, int[] dados)
        {
            int media = (dados[0] + dados[79]) / 2;
            int posicaoMedia = Array.IndexOf(dados, media);
            StringBuilder resposta = new StringBuilder($"-> { montaNumero(posicaoMedia, chaves, dados) }");
            for (int posicao = posicaoMedia + 1; posicao < posicaoMedia + 3; posicao++)
            {
                resposta.Append(montaNumero(posicao, chaves, dados));
            }
            int posicaoExtra = posicaoMedia + 2;
            while (dados[posicaoExtra] == dados[posicaoExtra + 1])
            {
                resposta.Append(montaNumero(posicaoExtra + 1, chaves, dados));
                posicaoExtra++;
            }
            for (int posicao = posicaoMedia - 1; posicao > posicaoMedia - 3; posicao--)
            {
                resposta.Insert(0, montaNumero(posicao, chaves, dados));
            }

            posicaoExtra = posicaoMedia - 2;
            while (dados[posicaoExtra] == dados[posicaoExtra - 1])
            {
                resposta.Insert(0, montaNumero(posicaoExtra - 1, chaves, dados));
                posicaoExtra--;
            }

            resposta.Insert(0, $"\nMédio Sorteados - Média: { media } - Dezena: { chaves[posicaoMedia] + 1 }\n");
            return resposta.ToString();
        }

        private static string numerosMaisSorteados(int[] chaves, int[] dados)
        {
            StringBuilder resposta = new StringBuilder("\nMais Sorteados:\n");
            for (int posicao = dados.Length - 1; posicao > 74; posicao--)
            {
                resposta.Append(montaNumero(posicao, chaves, dados));
            }
            int indice = 75;
            while (dados[indice] == dados[indice - 1])
            {
                resposta.Append(montaNumero(indice - 1, chaves, dados));
                indice--;
            }
            return resposta.ToString();
        }

        private static string numerosMenosSorteados(int[] chaves, int[] dados)
        {
            StringBuilder resposta = new StringBuilder("\n\nMenos Sorteados:\n");
            for (int posicao = 0; posicao < 5; posicao++)
            {
                resposta.Append(montaNumero(posicao, chaves, dados));
            }
            int indice = 4;
            while (dados[indice] == dados[indice + 1])
            {
                resposta.Append(montaNumero(indice + 1, chaves, dados));
                indice++;
            }
            return resposta.ToString(); ;
        }

        static string converteVetorImpressao(int[] vetorChaves, int[] vetorDados)
        {
            int[] quebras = new int[] { 7, 15, 23, 31, 39, 47, 55, 63, 71 };
            StringBuilder impressao = new StringBuilder("\n ");
            for (int posicao = 0; posicao < vetorChaves.Length; posicao++)
            {
                impressao.Append($"{ string.Format("{0,2}", vetorChaves[posicao] + 1) }: { string.Format("{0,3}", vetorDados[posicao]) }");
                string separador = quebras.Contains(posicao) ? "\n" : "   ";
                impressao.Append($" {separador} ");
            }
            return impressao.ToString();
        }
    }
}
