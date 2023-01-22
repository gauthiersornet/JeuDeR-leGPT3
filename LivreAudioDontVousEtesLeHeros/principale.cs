using Microsoft.CognitiveServices.Speech;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vosk;
using System.Security.Cryptography;
using System.Xml;
using Universal.OpenAI.Client;
using System.Threading;

namespace LivreAudioDontVousEtesLeHeros
{
    public partial class principale : Form
    {
        static private readonly string clée = null;
        static private readonly string région = null;

        private int entréeAudio;
        private int sortieAudio;

        private WaveIn waveIn = null;
        private WaveOut waveOut = null;
        private Model sttModel = null;
        private VoskRecognizer vRecognizer = null;
        private SpeechSynthesizer speechSynthesizer = null;

        private CopContexte copContex = null;
        private bool etatChoix = false;
        private Task<CreateCompletionResponse> tskCreatResp;
        private CancellationToken cclTkn;

        private readonly string cacheAudio = "CacheAudio";

        public principale()
        {
            InitializeComponent();

            //MotsCle.Phonem[] pha = MotsCle.Phonemiser("hello");
            //MotsCle.Phonem[] phb = MotsCle.Phonemiser("helo");
            //float d = MotsCle.distance(pha, phb);

            if (!Directory.Exists(cacheAudio)) Directory.CreateDirectory("CacheAudio");

            entréeAudio = 0;
            sortieAudio = 0;

            // You can set to -1 to disable logging messages
            Vosk.Vosk.SetLogLevel(0);
            sttModel = new Model("vosk-model-small-fr-0.22");

            //GénérerCache();
            //LireMP3("TextToSpeech.mp3");
            Dire("Veuillez glisser et déposer le début de votre histoire.");
        }

        private void OuvrirAudioHP()
        {
            try
            {
                if (WaveOut.DeviceCount > 0)
                {
                    if (sortieAudio < 0 || WaveOut.DeviceCount <= sortieAudio) sortieAudio = 0;
                }
                waveOut = new WaveOut();
                waveOut.DeviceNumber = sortieAudio;
                waveOut.PlaybackStopped += OnAudioHPStop;
            }
            catch
            {
                waveOut = null;
            }
        }

        private void FermerAudioHP()
        {
            WaveOut wo = waveOut;
            waveOut = null;
            wo.PlaybackStopped -= OnAudioHPStop;
            //wo.Dispose();
        }

        private void ActiverAudioMicro()
        {
            if (WaveIn.DeviceCount > 0)
            {
                if (entréeAudio < 0 || WaveIn.DeviceCount <= entréeAudio) entréeAudio = 0;
            }
            else entréeAudio = -1;
            if (entréeAudio >= 0)
            {
                waveIn = new WaveIn(this.Handle);
                waveIn.BufferMilliseconds = 2000;
                waveIn.DeviceNumber = entréeAudio;
                waveIn.WaveFormat = new WaveFormat(16000, 16, 1);
                waveIn.DataAvailable += OnAudioCaptured;
                waveIn.StartRecording();
            }
        }

        public void evt_MessageJR(string message, bool ivk)
        {
            txtBx.AppendText("Héro : " + message + "\r\n");
            MajScroll();
        }

        public void evt_MessageMJ(string message, bool ivk)
        {
            CancellationToken tk = cclTkn;
            Task<CreateCompletionResponse> svTsk = tskCreatResp;
            if (svTsk != null)
            {
                tskCreatResp = null;
                if (ivk)
                {
                    try
                    {
                        //if (!svTsk.Wait(1000, tk))
                        {
                            if (!tk.IsCancellationRequested)
                            {
                                tk.ThrowIfCancellationRequested();
                            }
                            svTsk.Wait();
                        }
                    }
                    catch { }
                }
            }
            if (!GénérerDire(message)) ActiverAudioMicro();
        }

        //int hyster = 0;
        void OnAudioCaptured(object sender, WaveInEventArgs e)
        {
            if (timeOut.Enabled) timeOut.Enabled = false;
            /*byte[] bff = e.Buffer;
            int len = e.BytesRecorded;
            short hmax = 0;
            for (int i = 0; i < len; i += 2)
            {
                short h = BitConverter.ToInt16(bff, i);
                if (h < 0)
                {
                    if (h == short.MinValue)
                    {
                        hmax = short.MaxValue;
                        break;
                    }
                    else if (-h > hmax) hmax = (short)-h;
                }
                else if (h > hmax) hmax = h;
            }
            if (hmax > 200)
            {*/
            //if (hmax > 3000) hyster = 4;
            //if (hyster > 0)
            //{
            //--hyster;
            //e.BytesRecorded
            if (vRecognizer == null)
            {
                SpkModel spkModel = new SpkModel("model-spk");
                vRecognizer = new VoskRecognizer(sttModel, 16000.0f);
                vRecognizer.SetSpkModel(spkModel);
            }
                    
            if (vRecognizer.AcceptWaveform(e.Buffer, e.BytesRecorded))
            {
                //txtBx.AppendText(vRecognizer.Result() + "\r\n");
                string stt = vRecognizer.FinalResult();
                if (stt.Length > "{\n  \"text\" : \"".Length + "\"\n}".Length)
                {
                    //"{\n  \"text\" : \"test\"\n}"
                    MessageSTTIn(stt.Substring("{\n  \"text\" : \"".Length, stt.Length - "{\n  \"text\" : \"".Length - "\"\n}".Length));
                }
                vRecognizer.Reset();
            }
            else
            {
                //txtBx.AppendText(vRecognizer.PartialResult() + "\r\n");
                timeOut.Enabled = true;
            }
                //}
            //}
            //else if (hyster > 0) --hyster;
        }

        private void MessageSTTIn(string stt)
        {
            //txtBx.AppendText("Vous : " + stt + "\r\n");
            //MajScroll();
            //FermerAudioMicro();
            //waveIn.StopRecording();
            
            //waveIn.DataAvailable -= OnAudioCaptured;
            //Dire("Vous avez dit : " + stt);
            if(copContex != null && etatChoix)
            {
                etatChoix = false;
                FermerAudioMicro();

                CancellationToken tk = cclTkn;
                Task<CreateCompletionResponse> svTsk = tskCreatResp;
                if (svTsk != null)
                {
                    tskCreatResp = null;
                    try
                    {
                        //if (!svTsk.Wait(1000, tk))
                        {
                            if (!tk.IsCancellationRequested)
                            {
                                tk.ThrowIfCancellationRequested();
                            }
                            svTsk.Wait();
                        }
                    }
                    catch { }
                }

                (tskCreatResp, cclTkn) = copContex.DemanderAsync(stt);
            }
            //if (livre != null && livre.Etape != null && livre.Etat == Livre.EEtat.Choix)
            //{
            //    if(livre.EtapeSuivante(stt) && livre.Etape != null)
            //    {
            //        livre.Etat = Livre.EEtat.Etape;
            //        if (!Narrer(livre.Etape, livre.Chemin)) OnAudioHPStop(this, null);
            //    }
            //    else if(!string.IsNullOrEmpty(livre.Etape.Options))
            //    {
            //        livre.Etat = Livre.EEtat.Options;
            //        if(!Lire(livre.Etape.Options, livre.Chemin + "Opts_" + livre.Etape.Identifiant + ".mp3")) OnAudioHPStop(this, null);
            //    }
            //}
            //else Dire("Veuillez glisser et déposer un livre.");
            //Lire(livre.Etape, livre.Chemin);
            //waveIn.DataAvailable += OnAudioCaptured;
            //waveIn.StartRecording();
            //waveIn.BufferMilliseconds = 2000;
            //waveIn.DeviceNumber = entréeAudio;
            //waveIn.WaveFormat = new WaveFormat(16000, 16, 1);
            //waveIn.DataAvailable += OnAudioCaptured;
            //waveIn.StartRecording();
        }

        private byte[] Synthétiser(string tts)
        {
            if(speechSynthesizer == null)
            {
                if (string.IsNullOrWhiteSpace(clée) || string.IsNullOrWhiteSpace(région)) return null;
                SpeechConfig speechConfig = SpeechConfig.FromSubscription(clée, région);
                speechConfig.SpeechSynthesisLanguage = "fr-FR";
                speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio48Khz96KBitRateMonoMp3);
                speechSynthesizer = new SpeechSynthesizer(speechConfig);
            }
            lock (speechSynthesizer)
            {
                Task<SpeechSynthesisResult> tache = speechSynthesizer.SpeakTextAsync(tts);
                tache.Wait();
                byte[] rbuff = tache.Result.AudioData;
                tache.Result.Dispose();
                tache.Dispose();
                return rbuff;
            }
        }

        //public static void DemoShorts(Model model)
        //{
        //    // Demo float array
        //    VoskRecognizer rec = new VoskRecognizer(model, 16000.0f);
        //    using (Stream source = File.OpenRead("test.wav"))
        //    {
        //        byte[] buffer = new byte[4096];
        //        int bytesRead;
        //        while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
        //        {
        //            short[] fbuffer = new short[bytesRead / 2];
        //            for (int i = 0, n = 0; i < fbuffer.Length; i++, n += 2)
        //            {
        //                fbuffer[i] = BitConverter.ToInt16(buffer, n);
        //            }
        //            if (rec.AcceptWaveform(fbuffer, fbuffer.Length))
        //            {
        //                Console.WriteLine(rec.Result());
        //            }
        //            else
        //            {
        //                Console.WriteLine(rec.PartialResult());
        //            }
        //        }
        //    }
        //    Console.WriteLine(rec.FinalResult());
        //}

        //public static void DemoSpeaker(Model model)
        //{
        //    // Output speakers
        //    SpkModel spkModel = new SpkModel("model-spk");
        //    VoskRecognizer rec = new VoskRecognizer(model, 16000.0f);
        //    rec.SetSpkModel(spkModel);
        //
        //    using (Stream source = File.OpenRead("test.wav"))
        //    {
        //        byte[] buffer = new byte[4096];
        //        int bytesRead;
        //        while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
        //        {
        //            if (rec.AcceptWaveform(buffer, bytesRead))
        //            {
        //                Console.WriteLine(rec.Result());
        //            }
        //            else
        //            {
        //                Console.WriteLine(rec.PartialResult());
        //            }
        //        }
        //    }
        //    Console.WriteLine(rec.FinalResult());
        //}

        private void FermerAudioMicro()
        {
            timeOut.Enabled = false;
            WaveIn wvin = waveIn;
            waveIn = null;
            if (wvin != null)
            {
                wvin.StopRecording();
                wvin.DataAvailable -= OnAudioCaptured;
                //wvin.Dispose();
            }
        }

        private void MajScroll()
        {
            txtBx.SelectionStart = txtBx.TextLength;
            txtBx.ScrollToCaret();
        }

        public bool Lire(string tts, string fichier)
        {
            if(!string.IsNullOrWhiteSpace(tts))
            {
                txtBx.AppendText("Maître du jeu : " + tts + "\r\n");
                MajScroll();
                //string fic = chemin + "\\" + Livre.Sécuriser(tts) + ".mp3";
                if (!File.Exists(fichier)) //File.WriteAllBytes(fichier, Synthétiser(tts));
                {
                    byte[] aud = Synthétiser(tts);
                    if (aud != null && aud.Length > 0) File.WriteAllBytes(fichier, aud);
                    else return false;
                }
                return LireMP3(fichier);
            }
            else return false;
        }

        static public string Sécuriser(string str)
        {
            IEnumerable<Char> enm = str.Where(c => !(Char.IsLetterOrDigit(c)));
            foreach (Char c in enm) str = str.Replace(c, '_');
            return str;
        }

        public bool Dire(string tts)
        {
            if (!string.IsNullOrWhiteSpace(tts))
            {
                txtBx.AppendText("Maître du jeu : " + tts + "\r\n");
                MajScroll();
                string fic = cacheAudio + "\\" + Sécuriser(tts) + ".mp3";
                if (!File.Exists(fic))
                {
                    byte[] aud = Synthétiser(tts);
                    if (aud != null && aud.Length > 0) File.WriteAllBytes(fic, aud);
                    else return false;
                }
                return LireMP3(fic);
            }
            else return false;
        }

        public bool GénérerDire(string tts)
        {
            if (!string.IsNullOrWhiteSpace(tts))
            {
                txtBx.AppendText("Maître du jeu : " + tts + "\r\n");
                MajScroll();
                byte[] aud = Synthétiser(tts);
                return LireMP3(aud);
                //string fic = cacheAudio + "\\" + Livre.Sécuriser(tts) + ".mp3";
                //if (!File.Exists(fic))
                //{
                //    byte[] aud = Synthétiser(tts);
                //    if (aud != null && aud.Length > 0) File.WriteAllBytes(fic, aud);
                //    else return false;
                //}
                //return true;
            }
            else return false;
        }

        public bool LireMP3(byte[] mp3)
        {
            try
            {
                if (waveOut == null) OuvrirAudioHP();
                if (waveOut != null)
                {
                    MemoryStream memStrm = new MemoryStream(mp3);
                    Mp3FileReader reader = new Mp3FileReader(memStrm);
                    waveOut.Stop();
                    waveOut.Init(reader);

                    if (waveIn != null)
                    {
                        lock (waveIn)
                        {
                            FermerAudioMicro();
                        }
                    }
                    waveOut.Play();
                    return true;
                }
                else return false;
            }
            catch
            {
                return false;
            }
        }

        public bool LireMP3(string mp3fic)
        {
            try
            {
                if (waveOut == null) OuvrirAudioHP();
                if (waveOut != null)
                {
                    Mp3FileReader reader = new Mp3FileReader(mp3fic);
                    waveOut.Stop();
                    waveOut.Init(reader);

                    if (waveIn != null)
                    {
                        lock (waveIn)
                        {
                            FermerAudioMicro();
                        }
                    }
                    waveOut.Play();
                    return true;
                }
                else return false;
            }
            catch
            {
                return false;
            }
        }

        void OnAudioHPStop(object sender, StoppedEventArgs e)
        {
            if (copContex != null)
            {
                etatChoix = true;
                ActiverAudioMicro();
                //switch (livre.Etat)
                //{
                //    case Livre.EEtat.Titre:
                //        livre.Etat = Livre.EEtat.Etape;
                //        if(!Narrer(livre.Etape, livre.Chemin)) OnAudioHPStop(sender, e);
                //        break;
                //    case Livre.EEtat.Etape:
                //        livre.Etat = Livre.EEtat.Resultat;
                //        Etape.EEvènement evtVie = (Etape.EEvènement)((int)livre.Etape.Evènement & 3);
                //        if (livre.Héro != null && evtVie != Etape.EEvènement.Aucun)
                //        {
                //            switch(evtVie)
                //            {
                //                case Etape.EEvènement.MettreVie:
                //                    livre.Héro.Vie = livre.Etape.evènementVie;
                //                    break;
                //                case Etape.EEvènement.GagnerVie:
                //                    livre.Héro.Vie += livre.Etape.evènementVie;
                //                    break;
                //                case Etape.EEvènement.PerdreVie:
                //                    livre.Héro.Vie -= livre.Etape.evènementVie;
                //                    break;
                //            }
                //            Dire("Il vous reste " + Math.Max(0, livre.Héro.Vie) + " vie.");
                //        }
                //        else OnAudioHPStop(sender, e);
                //        break;
                //    case Livre.EEtat.Resultat:
                //        if (livre.Héro != null && livre.Héro.Vie <= 0 || livre.Etape.Evènement.HasFlag(Etape.EEvènement.Perdre))
                //        {
                //            livre.Etat = Livre.EEtat.Perdre;
                //            Dire("Vous avez perdu.");
                //        }
                //        else if(livre.Etape.Evènement.HasFlag(Etape.EEvènement.Gagner))
                //        {
                //            livre.Etat = Livre.EEtat.Gagner;
                //            Dire("Vous avez gagné, félicitation.");
                //        }
                //        else
                //        {
                //            livre.Etat = Livre.EEtat.Options;
                //            if (!string.IsNullOrWhiteSpace(livre.Etape.Options))
                //            {
                //                if(!Lire(livre.Etape.Options, livre.Chemin + "Opts_" + livre.Etape.Identifiant + ".mp3")) OnAudioHPStop(sender, e);
                //            }
                //            else OnAudioHPStop(sender, e);
                //        }
                //        break;
                //    case Livre.EEtat.Options:
                //        livre.Etat = Livre.EEtat.Choix;
                //        if (waveIn == null) ActiverAudioMicro();
                //        break;
                //    case Livre.EEtat.Gagner: break;
                //    case Livre.EEtat.Perdre: break;
                //    default: if (waveIn == null) ActiverAudioMicro();break;
                //}
            }
        }

        private void principale_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void principale_DragDrop(object sender, DragEventArgs e)
        {
            string[] lstFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (lstFiles != null && lstFiles.Length > 0)
            {
                string fic = lstFiles.FirstOrDefault(f => f.TrimEnd().ToUpper().EndsWith(".TXT"));
                if (fic !=  null)
                {
                    CancellationToken tk = cclTkn;
                    Task<CreateCompletionResponse> svTsk = tskCreatResp;
                    if (svTsk != null)
                    {
                        tskCreatResp = null;
                        try
                        {
                            //if (!svTsk.Wait(1000, tk))
                            {
                                if (!tk.IsCancellationRequested)
                                {
                                    tk.ThrowIfCancellationRequested();
                                }
                                svTsk.Wait();
                            }
                        }
                        catch { }
                    }
                    if (copContex != null)
                    {
                        copContex.Dispose();
                        copContex = null;
                    }

                    etatChoix = false;

                    int l = fic.LastIndexOf('\\');
                    if (l < 0) l = fic.LastIndexOf('/');

                    string chemin;
                    if (l >= 0) chemin = fic.Substring(0, l + 1);
                    else chemin = "";

                    string debut = File.ReadAllText(fic);
                    try
                    {
                        debut = File.ReadAllText(fic);
                        CopContexte.SCopType cpt = new CopContexte.SCopType("jeux de rôle", null, null, CopContexte.EMoteur.TextDavinci003, "\r\nMJ : ", Color.DarkBlue, "\r\nH : ", Color.DarkGreen, true, null, null, null, null, 1024, new string[] { "H : ", "MJ : " }, "Ce qui suit est un jeu de rôle avec un mâitre du jeu d'MJ. Le mâitre du jeu est serviable, créatif et très amical. Il début l'aventure ainsi :\n\n" + debut);
                        copContex = new CopContexte(cpt, txtApk.Text, this);
                        GénérerDire(debut);
                    }
                    catch (System.IO.FileNotFoundException ex)
                    {
                        Dire("Votre fichier d'histoire n'a pas été trouvé.");
                        //MessageBox.Show(this, ex.Message, "Fichier non trouvé", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            this.ActiveControl = null;
        }

        private void timeOut_Tick(object sender, EventArgs e)
        {
            if (vRecognizer != null)
            {
                string stt = vRecognizer.FinalResult();
                if (stt.Length > "{\n  \"text\" : \"".Length + "\"\n}".Length)
                {
                    //"{\n  \"text\" : \"test\"\n}"
                    MessageSTTIn(stt.Substring("{\n  \"text\" : \"".Length, stt.Length - "{\n  \"text\" : \"".Length - "\"\n}".Length));
                }
                vRecognizer.Reset();
            }
        }

        //public void GénérerCache()
        //{
        //    GénérerDire("Veuillez glisser et déposer un livre.");
        //    for(int i = 0; i <= 20; ++i) GénérerDire("Il vous reste " + i + " vie.");
        //    GénérerDire("Vous avez perdu.");
        //    GénérerDire("Vous avez gagné, félicitation.");
        //    GénérerDire("Votre fichier livre n'a pas été trouvé.");
        //    GénérerDire("Votre fichier livre est inccorect.");
        //    GénérerDire("Ce livre est vide.");
        //}

        private void btPasser_Click(object sender, EventArgs e)
        {
            if (waveOut != null) waveOut.Stop();
        }
    }
}
