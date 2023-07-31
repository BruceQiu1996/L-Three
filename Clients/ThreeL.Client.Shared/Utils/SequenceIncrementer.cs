namespace ThreeL.Client.Shared.Utils
{
    public class SequenceIncrementer
    {
        private readonly int MaxSequence = int.MaxValue;
        private readonly int MinSequence = 0;
        public int CurrentSequence { get; private set; }

        public SequenceIncrementer()
        {
            CurrentSequence = 0;
        }

        public int GetNextSequence()
        {
            lock (this) 
            {
                CurrentSequence++;
                int temp = CurrentSequence;
                if (CurrentSequence >= MaxSequence)
                    CurrentSequence= MinSequence;

                return temp;
            }
        }
    }
}
