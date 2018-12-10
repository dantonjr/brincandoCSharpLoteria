using System;

namespace Estatisticas
{
    public class Resultado
    {
        public int[] DezenasJogo { get; private set; }
        public int[] Estatisticas { get; private set; }

        public Resultado(string dado)
        {
            string[] divisao = dado.Split(';');
            DezenasJogo = Array.ConvertAll(divisao[0].Split('-'), int.Parse);
            Array.Sort(DezenasJogo);
            Estatisticas = Array.ConvertAll(new string[] { divisao[1], divisao[2], divisao[3], divisao[4], divisao[5], divisao[6] }, int.Parse);
        }

        public void AtualizaEstatistica(int posicao)
        {
            Estatisticas[posicao]++;
        }

        public override string ToString()
        {
            return $"{ DezenasJogo[0] }-{ DezenasJogo[1] }-{ DezenasJogo[2] }-{ DezenasJogo[3] }-{ DezenasJogo[4] };{ Estatisticas[0] };{ Estatisticas[1] };{ Estatisticas[2] };{ Estatisticas[3] };{ Estatisticas[4] };{ Estatisticas[5] }";
        }
    }
}