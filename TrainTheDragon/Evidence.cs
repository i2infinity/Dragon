using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NLPToolkit;
using Sentiment.Analysis.Interfaces;

namespace DragonClassifier
{
    public class Evidence : IEvidence
    {
        private static readonly log4net.ILog Logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Dictionary<string, double> _evidences;
        private readonly string _evidenceRepository;
        private readonly string _evidenceFileName;
        private readonly string _evidenceFilePath;
        private const char EVIDENCE_SEPARATOR = ',';
        private double _totalWords;
        private readonly string _className;
        private readonly bool _saveEvidence;

        public Evidence(string className, string evidenceFilePath, bool loadEvidence = true, bool saveEvidence = false)
        {
            if (string.IsNullOrWhiteSpace(className)) throw new Exception("Class name was not defined");

            _saveEvidence = saveEvidence;
            _className = className;
            _evidences = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            _evidenceFileName = evidenceFilePath;

            Logger.InfoFormat("[DragonClassifier.Evidence.Ctor] Creating evidence instance for " + className);

            if (loadEvidence) LoadEvidenceFromCache(evidenceFilePath);
        }

        ~Evidence()
        {
            if (_saveEvidence) PersistEvidence();
        }

        public void LoadEvidenceFromCache(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return;

                Logger.Debug("Loading evidence cache from " + filePath);

                using (var file = new StreamReader(filePath))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        var keyValue = line.Split(EVIDENCE_SEPARATOR);
                        if (!_evidences.ContainsKey(keyValue[0]))
                        {
                            var value = int.Parse(keyValue[1]);
                            _evidences.Add(keyValue[0], value);
                            _totalWords += value;
                        }
                        else
                        {
                            throw new Exception(
                                string.Format("Duplicate entries of {0} found while loading evidences from {1}",
                                              keyValue[0], filePath));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error while loading evidence from cache", ex);
                throw;
            }
        }

        public bool AddEvidenceData(string trainingData, HashSet<string> wordsToIgnore)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(trainingData)) return false;

                var tokens = Tokenizer.TokenizeNow(trainingData).ToList();

                foreach (
                    var token in
                        tokens.Where(token => !string.IsNullOrWhiteSpace(token) && !wordsToIgnore.Contains(token)))
                {
                    if (!_evidences.ContainsKey(token))
                        _evidences[token] = 0;

                    _evidences[token]++;
                    _totalWords++;
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to load evidence data " + trainingData, ex);
                throw;
            }
        }

        public bool AddEvidenceData(IEnumerable<string> trainingData, HashSet<string> wordsToIgnore)
        {
            foreach (var data in trainingData)
            {
                AddEvidenceData(data, wordsToIgnore);
            }

            return true;
        }

        public IDictionary<string, double> GetEvidence()
        {
            return _evidences;
        }

        public bool PersistEvidence(bool backupExisting = true)
        {
            try
            {
                if (_evidences == null || _evidences.Count <= 0)
                    return false;

                if (backupExisting)
                    DragonHelper.DragonHelper.BackupFile(_evidenceFilePath);

                using (var file = new StreamWriter(_evidenceFilePath))
                {
                    foreach (
                        var line in
                            _evidences.Select(
                                evidence =>
                                evidence.Key.Trim() + "," + evidence.Value.ToString(CultureInfo.InvariantCulture).Trim())
                        )
                    {
                        file.WriteLine(line);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error while storing evidence data to " + _evidenceFilePath, ex);
                throw;
            }

            return true;
        }

        public string Name()
        {
            return _className;
        }

        public double TotalWords()
        {
            return _totalWords;
        }
    }
}