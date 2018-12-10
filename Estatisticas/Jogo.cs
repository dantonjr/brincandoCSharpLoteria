using System;

namespace Estatisticas
{
    public class Jogo
    {

        public int NumJogo { get; private set; }
        public DateTime DataJogo { get; private set; }
        public int[] DezenasJogo { get; private set; }

        public Jogo(string numJogo, string dataJogo, string[] dezenasJogo)
        {
            NumJogo = Convert.ToInt32(numJogo);
            DataJogo = Convert.ToDateTime(dataJogo);
            DezenasJogo = Array.ConvertAll(dezenasJogo, int.Parse);
            Array.Sort(DezenasJogo);
        }

        public override string ToString()
        {
            return $"{ NumJogo };{ DataJogo.ToShortDateString() };{ DezenasJogo[0] };{ DezenasJogo[1] };{ DezenasJogo[2] };{ DezenasJogo[3] };{ DezenasJogo[4] }";
        }
    }
}
