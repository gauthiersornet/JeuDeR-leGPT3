using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Universal.Common.Net.Http;
using Universal.OpenAI.Client;

namespace LivreAudioDontVousEtesLeHeros
{
    public class CopContexte : IDisposable
    {
        public enum EMoteur
        {
            Vide = 0,
            TextAda001,
            TextBabbage001,
            TextCurie001,
            TextDavinci003,
            CodeDavinci002,
            CodeCushman001
        }

        public struct SMoteur
        {
            public EMoteur moteur;
            public string nomMoteur;
            public int nombre_jetons_max;
            public SMoteur(EMoteur _moteur, string _nomMoteur, int _nombre_jetons_max)
            {
                moteur = _moteur;
                nomMoteur = _nomMoteur;
                nombre_jetons_max = _nombre_jetons_max;
            }
        }

        static public readonly SMoteur[] tabMoteur =
        {
            new SMoteur(EMoteur.Vide, null, 0),
            new SMoteur(EMoteur.TextAda001 ,"text-ada-001", 2048),
            new SMoteur(EMoteur.TextBabbage001 ,"text-babbage-001", 2048),
            new SMoteur(EMoteur.TextCurie001 ,"text-curie-001", 2048),
            new SMoteur(EMoteur.TextDavinci003 ,"text-davinci-003", 4000),
            new SMoteur(EMoteur.CodeDavinci002 ,"code-davinci-002", 4000),
            new SMoteur(EMoteur.CodeCushman001 ,"code-cushman-001", 2048)
        };

        public struct SCopType
        {
            public string categorieCopType;
            public string nomCopType;
            public string langage_défaut;
            public EMoteur eMoteur;
            public string prefix; //Au début de la complétion soit ajouté avant la complétion
            public Color? couleur_réponse;
            public string suffix; //A la fin de la complétion soit ajouté après la complétion
            public Color? couleur_demande;
            public bool echo;
            public float? temperature;
            public float? topP;
            public float? frequencyPenalty;
            public float? presencePenalty;
            public int? bestOf;
            public int nombre_gen_max;
            public string[] stop;
            public string copDémarreur;
            public SCopType(string _categorieCopType, string _nomCopType, string _langage_défaut, EMoteur _moteur, string _prefix, Color? _couleur_réponse, string _suffix, Color? _couleur_demande, bool _echo, float? _temperature, int? _topP, float? _frequencyPenalty, float? _presencePenalty, int _nombre_gen_max, string[] _stop, string _copDémarreur)
            {
                categorieCopType = _categorieCopType;
                nomCopType = _nomCopType;
                langage_défaut = _langage_défaut;
                eMoteur = _moteur;
                prefix = _prefix;
                couleur_réponse = _couleur_réponse;
                suffix = _suffix;
                couleur_demande = _couleur_demande;
                echo = _echo;
                temperature = _temperature;
                topP = _topP;
                frequencyPenalty = _frequencyPenalty;
                presencePenalty = _presencePenalty;
                bestOf = null;
                nombre_gen_max = _nombre_gen_max;
                stop = _stop;
                copDémarreur = _copDémarreur;
            }

            public SMoteur moteur { get => tabMoteur[(int)eMoteur]; }
            public int nb_jeton_moteur { get => moteur.nombre_jetons_max; }

            public int ObtenirNombreDeJeton(string contexte, string demande)
            {
                return calculerNombreJeton(copDémarreur, contexte, suffix, demande, prefix);
            }

            public int ObtenirNombreDeJetonRestant(string contexte, string demande)
            {
                return moteur.nombre_jetons_max - ObtenirNombreDeJeton(echo ? contexte : null, demande);
            }

            public CreateCompletionRequest CréerDemandeDeComplétion(string demande, string nomUtilisateur = null)
            {
                SMoteur moteur = this.moteur;
                demande = copDémarreur + demande;
                int nb_jeton_restant = moteur.nombre_jetons_max - calculerNombreJeton(demande);
                if (nb_jeton_restant >= 0)
                {
                    return new CreateCompletionRequest()
                    {
                        Model = moteur.nomMoteur,
                        Suffix = suffix,
                        Temperature = temperature,
                        TopP = topP,
                        FrequencyPenalty = frequencyPenalty,
                        PresencePenalty = presencePenalty,
                        BestOf = bestOf,
                        MaxTokens = Math.Min(nombre_gen_max, nb_jeton_restant),
                        Stop = stop,
                        Prompt = demande,
                        User = nomUtilisateur
                    };
                }
                else return null;
            }

            static public string RetirerDeEchange(string prefix, string echange, string suffix)
            {
                if (prefix != null)
                {
                    if (echange.StartsWith(prefix))
                    {
                        echange = echange.Substring(prefix.Length);
                    }
                    else if (prefix.StartsWith("\r\n"))
                    {
                        if (echange.StartsWith("\r\n"))
                            echange = echange.Substring(2);
                        else if (echange.StartsWith(prefix.Substring(2)))
                            echange = echange.Substring(prefix.Length - 2);
                    }
                }
                if (suffix != null)
                {
                    if(echange.EndsWith(suffix))
                    {
                        echange = echange.Substring(0, echange.Length - suffix.Length);
                    }
                    else if (suffix.StartsWith("\r\n"))
                    {
                        if (echange.EndsWith("\r\n"))
                            echange = echange.Substring(0, echange.Length - 2);
                        else if (echange.EndsWith(suffix.Substring(2)))
                            echange = echange.Substring(0, echange.Length - (suffix.Length - 2));
                    }
                }
                return echange;
            }

            static public string FiltrerEchange(string prefix, string echange, string suffix)
            {
                if (prefix != null && !echange.StartsWith(prefix))
                {
                    if (prefix.StartsWith("\r\n"))
                    {
                        if (echange.StartsWith("\r\n"))
                            echange = prefix + echange.Substring(2);
                        else if (echange.StartsWith(prefix.Substring(2)))
                            echange = "\r\n" + echange;
                        else echange = prefix + echange;
                    }
                    else echange = prefix + echange;
                }
                if(suffix != null && !echange.EndsWith(suffix))
                {
                    if (suffix.StartsWith("\r\n"))
                    {
                        if (echange.EndsWith("\r\n"))
                            echange = echange + suffix.Substring(2);
                        else if (echange.EndsWith(suffix.Substring(2)))
                            echange = echange.Substring(0, echange.Length - (suffix.Length - 2)) + suffix;
                        else echange = echange + suffix;
                    }
                    else echange = echange + suffix;
                }
                return echange;
            }

            public string FiltrerContexte(string contexte)
            {
                return RetirerDeEchange(null, contexte, suffix);
            }

            public string FiltrerDemande(string demande)
            {
                return FiltrerEchange(suffix, demande, prefix);
            }

            public string FiltrerComplétion(string complétion)
            {
                return RetirerDeEchange(prefix, complétion, suffix);
            }

            static public string FiltrerConcat(string str1, string strc, string str2)
            {
                bool trimStart = false;
                if (str1 != null)
                {
                    if (str1.Length >= strc.Length)
                    {
                        if (str1.EndsWith(strc)) strc = "";
                        else if (str1 == "" || str1.EndsWith("\n")) trimStart = true;
                    }
                    else if (str1 == "" || str1.EndsWith("\n")) trimStart = true;
                }
                //else strc = strc.TrimStart();
                if (strc != "")
                {
                    if(str2 != null)
                    {
                        if(str2.Length >= strc.Length)
                        {
                            if (str2.StartsWith(strc)) strc = "";
                        }
                        //else if (str2 == "" || str2.StartsWith("\n")) strc = strc.TrimEnd();
                    }
                    //else strc = strc.TrimEnd();
                }
                if(trimStart) strc = strc.TrimStart();
                return strc;
            }
        }

        //static public readonly SCopType[] tabCopType =
        //{
        //    new SCopType("Chatter", "normal", null, EMoteur.TextDavinci003, "\r\nIA : ", Color.DarkBlue, "\r\nH : ", Color.DarkGreen, true, null, null, null, null, 1024, new string[]{ "H : ", "IA : " }, "Ce qui suit est une conversation avec un assistant d'IA. L'assistant est serviable, créatif, intelligent et très amical.\n\nH : Bonjour, qui êtes-vous ?\nIA : Je suis une IA créée par OpenAI. Comment puis-je vous aider aujourd'hui ?"),
        //    new SCopType("Chatter", "contrariant", null, EMoteur.TextDavinci003, "\r\nIA : ", Color.DarkBlue, "\r\nH : ", Color.DarkRed, true, null, null, null, null, 1024, new string[]{ "H : ", "IA : " }, "Marv est un chatbot qui répond à contrecœur à des questions par des réponses sarcastiques :\r\n\r\nVous : Combien de livres y a-t-il dans un kilogramme ?\r\nMarv : Encore cette question ? Il y a 2,2 livres dans un kilogramme. Veuillez en prendre note.\r\nVous : Que veut dire HTML ?\r\nMarv : Google était-il trop occupé ? Hypertext Markup Language. Le T est pour essayer de poser de meilleures questions à l'avenir.\r\nVous : Quand le premier avion a-t-il volé ?\r\nMarv : Le 17 décembre 1903, Wilbur et Orville Wright ont effectué les premiers vols. J'aimerais qu'ils viennent me chercher.\r\nVous : Quel est le sens de la vie ?\r\nMarv : Je ne suis pas sûr. Je vais demander à mon ami Google."),
        //    new SCopType("Résumer", null, null, EMoteur.TextDavinci003, null, Color.DarkBlue, null, Color.DarkGreen, false, 0.7f, null, null, null, 4000, null, "Résumez ceci pour un étudiant :\r\n\r\n"),
        //    new SCopType("Générique", null, null, EMoteur.TextDavinci003, null, Color.DarkBlue, null, Color.DarkGreen, true, null, null, null, null, 1024, null, ""),
        //    new SCopType("C#", "générateur", "C#", EMoteur.CodeCushman001, null, Color.DarkBlue, null, Color.DarkGreen, true, null, null, null, null, 2048, null, ""),
        //    new SCopType("C#", "explicateur", "C#", EMoteur.CodeCushman001, null, Color.DarkBlue, null, Color.DarkGreen, true, null, null, null, null, 2048, null, ""),
        //    new SCopType("C#", "correcteur", "C#", EMoteur.CodeCushman001, null, Color.DarkBlue, null, Color.DarkGreen, true, null, null, null, null, 2048, null, ""),
        //    new SCopType("C#", "commentateur", "C#", EMoteur.CodeCushman001, null, Color.DarkBlue, null, Color.DarkGreen, true, null, null, null, null, 2048, null, "")
        //};

        public SCopType CopType;
        public string Contexte;
        OpenAIClient openAIClient;
        private principale frm;

        public CopContexte(SCopType copType, string apikey, principale _frm, EMoteur moteur = EMoteur.Vide)
        {
            CopType = copType;
            Contexte = "";
            frm = _frm;
            if (moteur != EMoteur.Vide) CopType.eMoteur = moteur;
            openAIClient = new OpenAIClient(apikey);
            openAIClient.HttpResponseReceived += new EventHandler<Universal.Common.Net.Http.HttpResponseReceivedEventArgs>(httpResponseReceivedEvent);
        }

        public void ChangerAPK(string apikey)
        {
            openAIClient.Dispose();
            openAIClient = new OpenAIClient(apikey);
            openAIClient.HttpResponseReceived += new EventHandler<Universal.Common.Net.Http.HttpResponseReceivedEventArgs>(httpResponseReceivedEvent);
        }

        static public int calculerNombreJeton(string str)
        {
            if (str != null)
            {
                return (str.Length + 3) / 4;
            }
            else return 0;
        }

        static public int calculerNombreJeton(params string[] strs)
        {
            int lenAcc = 0;
            foreach(string str in strs)
            {
                if(str != null) lenAcc += str.Length;
            }
            return (lenAcc + 3) / 4;
        }

        static public string verifierLimite(string demande, int nb_jeton_max)
        {
            if(demande.Length > 4 * nb_jeton_max)
            {
                demande = demande.Substring(demande.Length - 4 * nb_jeton_max);
            }
            return demande;
        }

        public string Demander(string demande, string utilisateur = null)
        {
            SMoteur moteur = tabMoteur[(int)CopType.eMoteur];
            string demande_complète;
            if(CopType.echo) demande_complète = verifierLimite(CopType.FiltrerContexte(Contexte) + CopType.FiltrerDemande(demande), moteur.nombre_jetons_max - calculerNombreJeton(CopType.copDémarreur));
            else
            {
                Contexte = "";
                demande_complète = verifierLimite(CopType.FiltrerDemande(demande), moteur.nombre_jetons_max - calculerNombreJeton(CopType.copDémarreur));
            }

            if (string.IsNullOrWhiteSpace(Contexte))
            {
                Contexte = "";
                if (demande_complète.StartsWith("\r\n")) demande_complète = demande_complète.Substring(2);
            }
            if (CopType.echo) Contexte = demande_complète;
            else Contexte = "";

            CreateCompletionRequest ccr = CopType.CréerDemandeDeComplétion(demande_complète, utilisateur);
            string completion;
            if (ccr != null)
            {
                ccr.Stream = false;
                var response = openAIClient.CreateCompletionAsync(ccr);
                response.Wait();
                completion = response.Result.FirstOrDefault().Text ?? "";
            }
            else completion = "Nombre maximum de jeton atteint => " + moteur.nombre_jetons_max + " jetons maximum.";

            TraiterComplétion(completion);
            return completion;
        }

        private string acc_completion;
        private delegate void dlgEvtMessage(string message, bool ivk);
        //private delegate void dlgEvtDernierMessage(int taille, string message, bool ivk);
        private void httpResponseReceivedEvent(object o, HttpResponseReceivedEventArgs httpresp)
        {
            var http = httpresp.Response.Content.ReadAsStringAsync();
            http.GetAwaiter().GetResult();
            if (http.Result.StartsWith("{\n    \"error\": {"))
            {
                if (frm.InvokeRequired)
                {
                    frm.BeginInvoke((dlgEvtMessage)frm.evt_MessageMJ, new object[] { "Erreur d'accès OpenAI.", true });
                }
                else
                {
                    frm.evt_MessageMJ(acc_completion, false);
                }
            }
            else
            {
                string[] strTab = http.Result.Split('\n');
                string sAcc = "";
                bool done = false;
                foreach (string s in strTab)
                {
                    if (s != "")
                    {
                        if (s == "data: [DONE]")
                        {
                            done = true;
                            break;
                        }
                        else
                        {
                            JObject js = JObject.Parse("{" + s + "}");
                            string r = js["data"]["choices"][0]["text"].ToString();
                            sAcc += r;
                        }
                    }
                }

                acc_completion += sAcc;
                //if (frm.InvokeRequired)
                //{
                //    frm.BeginInvoke((dlgEvtMessage)frm.evt, new object[] { sAcc, true });
                //}
                //else
                //{
                //    frm.evt_NouveauMessage(sAcc, false);
                //}

                if (done)
                {
                    TraiterComplétion(acc_completion);
                    if (frm.InvokeRequired)
                    {
                        frm.BeginInvoke((dlgEvtMessage)frm.evt_MessageMJ, new object[] { acc_completion, true });
                    }
                    else
                    {
                        frm.evt_MessageMJ(acc_completion, false);
                    }
                }
            }
        }

        public (Task<CreateCompletionResponse>, CancellationToken) DemanderAsync(string demande, string utilisateur = null)
        {
            //if (!string.IsNullOrWhiteSpace(CopType.suffix))
            //{
            //    frm.evt_MessageJR(SCopType.FiltrerConcat(Contexte, CopType.suffix, demande), false);
            //}
            if (!string.IsNullOrWhiteSpace(demande))
            {
                frm.evt_MessageJR(demande, false);
            }
            //if (!string.IsNullOrWhiteSpace(CopType.prefix))
            //{
            //    frm.evt_MessageJR(SCopType.FiltrerConcat(demande, CopType.prefix, null), false);
            //}

            SMoteur moteur = tabMoteur[(int)CopType.eMoteur];
            string demande_complète;
            if (CopType.echo) demande_complète = verifierLimite(CopType.FiltrerContexte(Contexte) + CopType.FiltrerDemande(demande), moteur.nombre_jetons_max - calculerNombreJeton(CopType.copDémarreur));
            else
            {
                Contexte = "";
                demande_complète = verifierLimite(CopType.FiltrerDemande(demande), moteur.nombre_jetons_max - calculerNombreJeton(CopType.copDémarreur));
            }

            if (string.IsNullOrWhiteSpace(Contexte))
            {
                Contexte = "";
                if (demande_complète.StartsWith("\r\n")) demande_complète = demande_complète.Substring(2);
            }
            if (CopType.echo) Contexte = demande_complète;

            CreateCompletionRequest ccr = CopType.CréerDemandeDeComplétion(demande_complète, utilisateur);
            string completion;
            if (ccr != null)
            {
                acc_completion = "";
                ccr.Stream = true;
                CancellationToken cancellation = new System.Threading.CancellationToken(false);
                Task<CreateCompletionResponse> t = openAIClient.CreateCompletionAsync(ccr, cancellation);
                return (t, cancellation);
            }
            else completion = "Nombre maximum de jeton atteint => " + moteur.nombre_jetons_max + " jetons maximum.";
            TraiterComplétion(completion);
            return (null, default(CancellationToken));
        }

        private void TraiterComplétion(string completion)
        {
            Contexte += CopType.FiltrerComplétion(completion);
            Contexte = Contexte.TrimStart();
            while (Contexte.StartsWith("\r") || Contexte.StartsWith("\n"))
            {
                Contexte = Contexte.Substring(1).TrimStart();
            }
        }

        public void Dispose()
        {
            openAIClient.Dispose();
            openAIClient = null;
        }
    }
}
