using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Markup;

namespace Indiceringsmodule.Common.DocumentObject
{
    public class Fact : Observable
    {
        #region Fields & Properties

        private protected int _ID;
        public int ID { get { return _ID; } }

        //document for selection and editing, containing (only)
        //text and links pertaining to a single fact
        private FlowDocument _FactDocument;
        public FlowDocument FactDocument
        {
            get { return _FactDocument; }
            set { SetProperty(ref _FactDocument, value); }
        }

        public Dictionary<Hyperlink, FactMember> TotalFactMembers;

        private Person _Person;
        public Person Person
        {
            get { return _Person; }
            set { SetProperty(ref _Person, value); }
        }

        private RealEstate _RealEstate;
        public RealEstate RealEstate
        {
            get { return _RealEstate; }
            set { SetProperty(ref _RealEstate, value); }
        }

        private Chattel _Chattel;
        public Chattel Chattel
        {
            get { return _Chattel; }
            set { SetProperty(ref _Chattel, value); }
        }

        #endregion

        #region Default Constructor

        public Fact(int id, string selection)
        {
            if (id < 0) throw new ArgumentOutOfRangeException($"*ID number cannot be a negative number. {id}");
            
            _ID = id;
            var fDoc = id + 1;
            FactDocument = new FlowDocument { Name = "Fact_Document_" + fDoc.ToString() };
            TotalFactMembers = new Dictionary<Hyperlink, FactMember>();

            PopulateFactDocument(selection);          
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clears the FactDocument, then populates it
        /// with a single Block of type Paragraph containing
        /// the selection text.
        /// This text is split on carriage returns (\r\n).
        /// Each text line is entered as an Inline, followed by a
        /// LineBreak Inline.
        /// </summary>
        /// <param name="selection"></param>
        private void PopulateFactDocument(string selection)
        {
            FactDocument.Blocks.Clear();
            var lines = Regex.Split(selection, "\r\n").ToList();
            var p = new Paragraph();
            foreach (var line in lines)
            {
                p.Inlines.Add(line);
                p.Inlines.Add(new LineBreak());
            }
            FactDocument.Blocks.Add(p);
        }

        public void CreatePerson(Hyperlink link)
        {
            var newID = TotalFactMembers.Count();
            TotalFactMembers.Add(link, new Person(newID, link));
        }

        public void CreateRealEstate(Hyperlink link)
        {
            var newID = TotalFactMembers.Count();
            TotalFactMembers.Add(link, new RealEstate(newID, link));
        }

        public void CreateChattel(Hyperlink link)
        {
            var newID = TotalFactMembers.Count();
            TotalFactMembers.Add(link, new Chattel(newID, link));
        }

        public FactMember GetFactMember(Hyperlink hyperlink)
        {
            TotalFactMembers.TryGetValue(hyperlink, out FactMember factMember);
            if (factMember == null)
            {
                throw new ArgumentNullException("*Could not find FactMember for key:" + hyperlink);
            }
            return factMember;
        }

        public int GetTotalNumberOfFactMembers()
        {
            return TotalFactMembers.Count();
        }

        /// <summary>
        /// Casts the first Inline of the Hyperlink as Run
        /// and attempts to extract the text string from it.
        /// If it fails, returns an empty string.
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        public string GetTextFromHyperlink(Hyperlink link)
        {
            return !(link.Inlines.FirstOrDefault() is Run run) ? string.Empty : run.Text;
        }

        public FactValidationFindings Validate()
        {
            var findings = new FactValidationFindings(this);
            var linksList = FindHyperlinks(FactDocument).ToList();
            
            if (linksList.Count != linksList.Distinct().Count())
            {
                var hash = new HashSet<Hyperlink>();

                foreach (var link in linksList)
                {
                    if (!hash.Add(link))
                    {
                        findings.duplicates.Add(link);
                        findings.areThereDuplicates = true;
                    }
                }
            }
            else
            {
                if (linksList.Count == TotalFactMembers.Keys.Count)
                {
                    IEnumerable<Hyperlink> matches = linksList.Intersect(TotalFactMembers.Keys);
                    if (matches.Count() == linksList.Count())
                    {
                        findings.allGreen = true;
                    }
                    else
                    {
                        findings.mismatch = true;
                        findings.linksListExceptions = linksList.Except(TotalFactMembers.Keys).ToList();
                        findings.dictExceptions = TotalFactMembers.Keys.Except(linksList).ToList();
                    }
                }
                else
                {
                    var greater = Math.Max(linksList.Count, TotalFactMembers.Keys.Count);
                    if (greater == linksList.Count)
                    {
                        findings.linkListHasMore = true;
                        findings.differenceCount = greater - TotalFactMembers.Keys.Count;
                        findings.linksListExceptions = linksList.Except(TotalFactMembers.Keys).ToList();
                    }
                    else if (greater == TotalFactMembers.Keys.Count)
                    {
                        findings.dictKeysHasMore = true;
                        findings.differenceCount = greater - linksList.Count;
                        findings.dictExceptions = TotalFactMembers.Keys.Except(linksList).ToList();
                    }
                }
            }
            return findings;
        }


        /// <summary>
        /// Scans a FlowDocument and returns all Hyperlinks(s)
        /// inside it as a a new IEnumerable collection.
        /// </summary>
        /// <param name="document"></param>
        /// <returns>collection of hyperlinks</returns>
        public IEnumerable<Hyperlink> FindHyperlinks(FlowDocument document)
        {
            return document.Blocks.SelectMany(block => FindhLinks(block));
        }

        /// <summary>
        /// Helper method that scans each block in search of Hyperlinks
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static IEnumerable<Hyperlink> FindhLinks(Block block)
        {
            if (block is Table)
            {
                return ((Table)block).RowGroups
                    .SelectMany(x => x.Rows)
                    .SelectMany(x => x.Cells)
                    .SelectMany(x => x.Blocks)
                    .SelectMany(innerBlock => FindhLinks(innerBlock));
            }
            if (block is Paragraph)
            {
                return ((Paragraph)block).Inlines

                    .Where(x => x is Hyperlink)
                    .Select(x => x as Hyperlink);
            }
            if (block is List)
            {
                return ((List)block).ListItems.SelectMany(listItem => listItem
                                                                      .Blocks
                                                                      .SelectMany(innerBlock => FindhLinks(innerBlock)));
            }
            throw new InvalidOperationException("Unknown block type: " + block.GetType());
        }

        /// <summary>
        /// Returns a string based on the nature of FactValidationFindings provided
        /// </summary>
        /// <param name="finding">the assessed state of the Fact</param>
        /// <returns></returns>
        public string ResolveLinkListHasMoreMessage(FactValidationFindings finding)
        {
            if (finding.linksListExceptions.Count == 1)
            {
                var linkText = finding.fact.GetTextFromHyperlink(finding.linksListExceptions.FirstOrDefault());
                var factNr = finding.fact.ID + 1;
                return string.Format(Language.Resources.ListHasMessage1, linkText, factNr.ToString()) + "\r\n";
            }
            else if (finding.linksListExceptions.Count > 1)
            {
                var hyperLinkStrings = new List<string>();
                foreach (var link in finding.linksListExceptions)
                {
                    var linkText = finding.fact.GetTextFromHyperlink(link);
                    hyperLinkStrings.Add(linkText);
                }
                var sb = new StringBuilder();
                for (int i = 0; i < hyperLinkStrings.Count - 1; i++)
                {
                    sb.AppendLine(i.ToString() + ", ");
                }
                return string.Format(Language.Resources.ListHasMessage2, sb) + "\r\n";
            }
            else
            {
                throw new ArgumentOutOfRangeException("linkListExceptions cannot be empty.");
            }
        }

        /// <summary>
        /// Returns a string based on the nature of FactValidationFindings provided
        /// </summary>
        /// <param name="finding">the assessed state of the Fact</param>
        /// <returns></returns>
        public string ResolveDictKeysHasMoreMessage(FactValidationFindings finding)
        {
            if (finding.dictExceptions.Count == 1)
            {
                var link = finding.dictExceptions.FirstOrDefault();
                var run = link.Inlines.FirstOrDefault() as Run;
                finding.fact.RestoreLink(finding.fact, link);
                var factNr = finding.fact.ID + 1;
                return string.Format(Language.Resources.DictHasMessage1, factNr.ToString(), run.Text) + "\r\n";                
            }
            else if (finding.dictExceptions.Count > 1)
            {
                var hyperLinkStrings = new List<string>();
                foreach (var link in finding.dictExceptions)
                {
                    var linkText = finding.fact.GetTextFromHyperlink(link);
                    hyperLinkStrings.Add(linkText);
                }
                var sb = new StringBuilder();
                for (int i = 0; i < hyperLinkStrings.Count - 1; i++)
                {
                    sb.AppendLine(i.ToString() + ", ");
                }
                return string.Format(Language.Resources.DictHasMessage2, sb) + "\r\n";
            }
            else
            {
                throw new ArgumentOutOfRangeException("dictExceptions cannot be empty.");
            }
        }

        /// <summary>
        /// Returns a string based on the nature of mismatches in finding
        /// </summary>
        /// <param name="finding">the assessed state of the Fact</param>
        /// <returns></returns>
        public string ResolveMismatchMessage(FactValidationFindings finding)
        {
            if (finding.linksListExceptions.Count == 1)
            {
                var linkText = finding.fact.GetTextFromHyperlink(finding.linksListExceptions.FirstOrDefault());
                var factNr = finding.fact.ID + 1;
                return string.Format(Language.Resources.MismatchMessage1, linkText, factNr.ToString()) + "\r\n";
            }
            else if (finding.linksListExceptions.Count > 1)
            {
                var hyperLinkStrings = new List<string>();
                foreach (var link in finding.linksListExceptions)
                {
                    var linkText = finding.fact.GetTextFromHyperlink(link);
                    hyperLinkStrings.Add(linkText);
                }
                var sb = new StringBuilder();
                for (int i = 0; i < hyperLinkStrings.Count - 1; i++)
                {
                    sb.AppendLine(i.ToString() + ", ");
                }
                return string.Format(Language.Resources.MismatchMessage2, sb) + "\r\n";
            }
            else
            {
                throw new ArgumentOutOfRangeException("linksListExceptions cannot be empty.");
            }
        }

        /// <summary>
        /// Returns a string based on the nature of duplications in finding
        /// </summary>
        /// <param name="finding">the assessed state of the Fact</param>
        /// <returns></returns>
        public string ResolveDuplicatesMessage(FactValidationFindings finding)
        {
            var hyperLinkStrings = new List<string>();
            foreach (var duplicate in finding.duplicates)
            {
                var linkText = finding.fact.GetTextFromHyperlink(duplicate);
                hyperLinkStrings.Add(linkText);
            }
            var sb = new StringBuilder();
            for (int i = 0; i < hyperLinkStrings.Count; i++)
            {
                sb.AppendLine(i.ToString() + ", ");
            }            
            return string.Format(Language.Resources.DuplicatesMessage, sb + "\r\n");
        }

        /// <summary>
        /// Appends the missing hyperlink to the bottom of the fact's
        /// fact document, together with a sting message.
        /// </summary>
        /// <param name="fact">The Fact that misses a link in its document to one of its FactMembers</param>
        /// <param name="link">The the link of the affected FactMember</param>
        private void RestoreLink(Fact fact, Hyperlink link)
        {
            var par = fact.FactDocument.Blocks.LastBlock as Paragraph;

            var run = new Run(Language.Resources.RestoredHyperlink);
            par.Inlines.Add(new LineBreak());
            par.Inlines.Add(run);
            par.Inlines.Add(link);
        }

        #endregion
    }
}
