using System.Collections.Generic;

namespace Sentiment.Analysis.Interfaces
{
    public interface IEvidence
    {
        string Name();
        double TotalWords();
        bool AddEvidenceData(string trainingData, HashSet<string> wordsToIgnore);
        bool AddEvidenceData(IEnumerable<string> trainingData, HashSet<string> wordsToIgnore);
        IDictionary<string, double> GetEvidence();
        bool PersistEvidence(bool backupExisting);
        void LoadEvidenceFromCache(string filePath);
    }
}
