/********************************************************************************
 *               Author: Christian Müller
 *      Date of cration: 2021-08-10                                       
 *   Copyright (C) 2022: christian Müller christian(at)chrmue(dot).de
 *
 *              Licence:
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 ********************************************************************************/

using libParser;
using System.Collections.Generic;
using System.Globalization;


namespace BatInspector
{

  public enum enSigStructure
  {
    FMa_CF_FMe,
    QCF_FM,
    QCF,
    FM_QCF,
    FM
  }

  public enum enBoolDC
  {
    YES,
    NO,
    DONT_CARE
  }

  public class CallParams
  {
    public enSigStructure SigStructure { get; set; }
    public double FME { get; set; }
    public double StartF { get; set; }
    public double EndF { get; set; }
    public double Duration { get; set; }
    public enBoolDC ProgressiveStart { get; set; }
    public enBoolDC ExplosiveStart { get; set; }

    public enBoolDC AbsenceOfPeek { get; set; }

    public enBoolDC FinalWack { get; set; }
    /// <summary>alternating with slightly higher QCF or FM_QCF signals</summary>
    public enBoolDC Alternating { get; set; }

    public enBoolDC NasalSonority { get; set; }
    public enBoolDC WhistledSonority { get; set; }
    public enBoolDC HockeyStick   { get; set; }

    public double Latitude { get; set;}
    public double Longitude { get; set;}
  }

  public class MissingInfo
  {
    public bool ProgressiveStart { get; set; }
    public bool ExplosiveStart { get; set; }

    public bool AbsenceOfPeek { get; set; }
    public bool FinalWack { get; set; }
    public bool Alternating { get; set; }

    public bool NasalSonority { get; set; }
    public bool WhistledSonority { get; set; }
    public bool HockeyStick { get; set; }

    public void init()
    {
      ProgressiveStart = false;
      ExplosiveStart = false;
      AbsenceOfPeek = false;
      FinalWack = false;
      Alternating = false;
      NasalSonority = false;
      WhistledSonority = false;
      HockeyStick = false;
    }
  }

  public enum enSpec
  {
    BBAR,    //Barbastella barbastellus (Mopsfledermaus)
    Eptesicus,
    EISA,    //Eptesicus isabellinus
    ENIL,    //Eptesicus nilsonii
    ESER,    //Eptesicus serotinus
    Hypsugo,
    HSAV,    //Hypsugo savii (Alpenfledermaus)
    Myotis,
    MALC,    //Myotis alcathoe 
    MBEC,    //Myotis bechsteinii (Bechsteinfedermaus)
    MBRA,    //Myotis brandtii (Große Bartfledermaus)
    MCAP,    //Myotis cappacinii
    MDAS,    //Myotis dasycneme (Teichfledermaus)
    MDAU,    //Myotis daubentonii (Wasserfledermaus)
    MEME,    //Myotis emarginatus (Wimpernfledermaus)
    MMYO,    //Myotis myotis (Großes Mausohr)
    MMYS,    //Myotis mystacinus
    MOXY,    //Myotis oxygnatus
    MNAT,    //Myotis nattereri (Fransenfledermaus)
    Minopterus,
    MSCH,    //Miniopterus schreibersii (Langfluegelfledermaus)
    Nyctalus,//Nyctalus spec.
    NLAS,    //Nyctalus lasiopterus
    NLEI,    //Nyctalus leisleri (Kleiner Abendsegler)
    NNOC,    //Nyctalus noctula (Großer Abendsegler)
    PAUR,    //Plecotus auritius (Brunes Langohr)
    PAUS,    //Plecotus austriacus (Graues Langohr)
    Plecotus,
    PKUH,    //Pipistrellus kuhlii 
    PNAT,    //Pipistrellus nathusii (Rauhautfledermaus)
    PPIP,    //Pipistrellus pipistrellus (Zwergfledermaus)
    PPYG,    //Pipistrellus pygmaeus (Mückenfledermaus)
    Pipstrellus,         //
    RFER,    //Rhinolophus ferrumequinum
    RBLA,    //Rhinolophus blasii
    RMEH,    //Rhinolophus mehelyi
    RHIP,    //Rhinolophus hipposideros
    REUR,    //Rhinolophus euryate
    TTEN,    //Tadarida teniotis
    Vespertilio,
    VMUR,    //Vespertilio murinus
    VSIN,    //Vespertilio sinensis
    UNKNOWN,
  }

 

  public class ClassifierBarataud
  {
    BatSpeciesRegions _batSpecRegions;

    public ClassifierBarataud(BatSpeciesRegions regions)
    {
      _batSpecRegions = regions;
    }

    public List<enSpec> classify(CallParams pars, ref MissingInfo info, out List<string> steps)
    {
      info.init();
      List<enSpec> retVal = new List<enSpec>();
      steps = new List<string>();

      ParRegion reg = _batSpecRegions.findRegion(pars.Latitude, pars.Longitude);
      if (reg != null)
         steps.Add("region: " + reg.Name);
      else
        steps.Add("region unrecognized");

      int step = 1;
      int lastStep = 1;
      while (retVal.Count == 0)
      {
        if (steps.Count > 10)
          retVal.Add(enSpec.UNKNOWN);
        string infoStr = "";
        switch (step)
        {
          case 1:
          case 2:
          case 3:
          case 4:
          case 5:
            switch (pars.SigStructure)
            {
              case enSigStructure.FMa_CF_FMe:
                step = 6;
                break;
              case enSigStructure.QCF_FM:
                step = 7;
                break;
              case enSigStructure.QCF:
                step = 8;
                break;
              case enSigStructure.FM_QCF:
                step = 13;
                break;
              case enSigStructure.FM:
                step = 16;
                break;
            }
            infoStr = "sigStruct: " + pars.SigStructure.ToString();
            break;

          case 6:
            if ((76 <= pars.FME) && (pars.FME <= 85) && occurrence(pars, enSpec.RFER))
              retVal.Add(enSpec.RFER);
            else if ((92 <= pars.FME) && (pars.FME <= 98) && occurrence(pars, enSpec.RBLA))
              retVal.Add(enSpec.RBLA);
            else if ((104 <= pars.FME) && (pars.FME <= 111) && occurrence(pars, enSpec.RMEH))
              retVal.Add(enSpec.RMEH);
            else if ((107 <= pars.FME) && (pars.FME <= 116) && occurrence(pars, enSpec.RHIP))
              retVal.Add(enSpec.RHIP);
            else if ((100 <= pars.FME) && (pars.FME <= 102) && occurrence(pars, enSpec.REUR))
              retVal.Add(enSpec.REUR);
            else if ((102 <= pars.FME) && (pars.FME <= 105) && occurrence(pars, enSpec.REUR)) 
              retVal.Add(enSpec.REUR);
            else if ((105 <= pars.FME) && (pars.FME <= 106) && occurrence(pars, enSpec.REUR))
              retVal.Add(enSpec.REUR);
            else if ((106 <= pars.FME) && (pars.FME <= 107) && occurrence(pars, enSpec.RHIP))
              retVal.Add(enSpec.RHIP);
            else
              retVal.Add(enSpec.UNKNOWN);
            infoStr = "FME: " + pars.FME;
            break;

          case 7:
            if ((4 <= pars.Duration) && (pars.Duration <= 10) &&
               (42 <= pars.FME) && (pars.FME <= 65) && occurrence(pars, enSpec.MDAU))
              retVal.Add(enSpec.MDAU);
            else if ((47 <= pars.FME) && (pars.FME <= 48) && occurrence(pars, enSpec.BBAR))
              retVal.Add(enSpec.BBAR);
            else
              retVal.Add(enSpec.UNKNOWN);
            infoStr = "duration: " + pars.Duration.ToString("#.#", CultureInfo.InvariantCulture) + " FME: " 
                      + pars.FME.ToString("#.#", CultureInfo.InvariantCulture);
            break;

          case 8:
            if (pars.FME <= 30)
              step = 9;
            else
              step = 12;
            infoStr = "FME: " + pars.FME.ToString("#.#", CultureInfo.InvariantCulture);
            break;

          case 9:
            if (pars.Alternating == enBoolDC.YES)
            {
              info.Alternating = true;
              step = 10;
            }
            else
              step = 11;
            infoStr = "Alternating: " + pars.Alternating.ToString();
            break;

          case 10:
            if ((13 < pars.FME) && (pars.FME < 17) && occurrence(pars, enSpec.NLAS))
              retVal.Add(enSpec.NLAS);
            else if ((17 <= pars.FME) && (pars.FME < 21) && occurrence(pars, enSpec.NNOC))
              retVal.Add(enSpec.NNOC);
            else if ((21 <= pars.FME) && (pars.FME < 27) && occurrence(pars, enSpec.NLEI))
              retVal.Add(enSpec.NLEI);
            else
              retVal.Add(enSpec.UNKNOWN);
            infoStr = "FME: " + pars.FME.ToString("#.#", CultureInfo.InvariantCulture);
            break;

          case 11:
            if ((9 <= pars.FME) && (pars.FME <= 12) && occurrence(pars, enSpec.TTEN))
              retVal.Add(enSpec.TTEN);
            else if ((21 <= pars.FME) && (pars.FME <= 25) && occurrence(pars, enSpec.EISA))
              retVal.Add(enSpec.EISA);
            else if ((22 <= pars.FME) && (pars.FME <= 26) &&
                     (18 <= pars.Duration) && (pars.Duration <= 24) && occurrence(pars, enSpec.VMUR))
              retVal.Add(enSpec.VMUR);
            else if ((27 < pars.FME) && (pars.FME <= 30) &&
                    (26.5 <= pars.EndF) && (pars.EndF < 29) && occurrence(pars, enSpec.ENIL))
              retVal.Add(enSpec.ENIL);
            else
              retVal.Add(enSpec.UNKNOWN);  //social call of NLEI not considered
            infoStr = "duration: " + pars.Duration.ToString("#.#", CultureInfo.InvariantCulture) +
                      " FME:" + pars.FME.ToString("#.#", CultureInfo.InvariantCulture);
            break;

          case 12:
            if ((30 <= pars.FME) && (pars.FME <= 34) && occurrence(pars, enSpec.HSAV))
              retVal.Add(enSpec.HSAV);
            else if ((35 <= pars.FME) && (pars.FME <= 38.5) && occurrence(pars, enSpec.PKUH))
              retVal.Add(enSpec.PKUH);
            else if ((39 <= pars.FME) && (pars.FME <= 42) && occurrence(pars, enSpec.PNAT))
              retVal.Add(enSpec.PNAT);
            else if ((42 < pars.FME) && (pars.FME <= 48) && occurrence(pars, enSpec.PPIP))
              retVal.Add(enSpec.PPIP);
            else if ((50 <= pars.FME) && (pars.FME <= 53) &&
                     (pars.Duration < 9) && occurrence(pars, enSpec.MSCH))
              retVal.Add(enSpec.MSCH);
            else if ((51 <= pars.FME) && (pars.FME <= 57) &&
                     (pars.Duration < 9) && occurrence(pars, enSpec.PPYG))
              retVal.Add(enSpec.PPYG);
            else
              retVal.Add(enSpec.UNKNOWN);
            infoStr = "duration: " + pars.Duration.ToString("#.#", CultureInfo.InvariantCulture) +
                      " FME:" + pars.FME.ToString("#.#", CultureInfo.InvariantCulture);
            break;

          case 13:
            if (pars.FME <= 30)
              step = 14;
            else
              step = 15;
            infoStr = "FME: " + pars.FME.ToString("#.#", CultureInfo.InvariantCulture);
            break;

          case 14:
            if ((24 <= pars.FME) && (pars.FME <= 27))
              retVal.Add(enSpec.ENIL);
            else if ((22 <= pars.FME) && (pars.FME <= 30))
            {
              bool found = false;
              if (occurrence(pars, enSpec.Eptesicus))
              {
                found = true;
                retVal.Add(enSpec.Eptesicus);
              }
              if (occurrence(pars, enSpec.Vespertilio))
              {
                found = true;
                retVal.Add(enSpec.Vespertilio);
              }
              if (occurrence(pars, enSpec.Nyctalus))
              {
                found = true;
                retVal.Add(enSpec.Nyctalus);
              }
              if (!found)
                retVal.Add(enSpec.UNKNOWN);
            }
            else
              retVal.Add(enSpec.UNKNOWN);
            infoStr = "FME: " + pars.FME.ToString("#.#", CultureInfo.InvariantCulture);
            break;
         
          case 15:
            if ((32 <= pars.FME) && (pars.FME <= 34) &&
                ((pars.StartF - pars.EndF) < 30) && occurrence(pars, enSpec.HSAV))
              retVal.Add(enSpec.HSAV);
            else if ((32 <= pars.FME) && (pars.FME <= 44) && (pars.EndF > 27) &&
                    ((pars.StartF - pars.EndF) > 30) && occurrence(pars, enSpec.ESER))
              retVal.Add(enSpec.ESER);
            else if ((37 <= pars.FME) && (pars.FME <= 39) && occurrence(pars, enSpec.PKUH))
              retVal.Add(enSpec.PKUH);
            //social call PNAT not considered
            else if ((44 <= pars.FME) && (pars.FME <= 50) && occurrence(pars, enSpec.PPIP))
              retVal.Add(enSpec.PPIP);
            else if ((50 <= pars.FME) && (pars.FME <= 56) &&
                     (pars.Duration > 9) && occurrence(pars, enSpec.MSCH))
              retVal.Add(enSpec.MSCH);
            else if ((52 <= pars.FME) && (pars.FME <= 64) &&
                     (pars.Duration < 9) && occurrence(pars, enSpec.PPYG))
              retVal.Add(enSpec.PPYG);
            else if ((33 <= pars.FME) && (pars.FME <= 35) &&
                     (10 <= pars.Duration) && (pars.Duration <= 18) &&
                      occurrence(pars, enSpec.MDAS))
              retVal.Add(enSpec.MDAS);
            else
              retVal.Add(enSpec.UNKNOWN);
            infoStr = "FME: " + pars.FME.ToString("#.#", CultureInfo.InvariantCulture) +
                      " duration:" + pars.Duration.ToString("#.#", CultureInfo.InvariantCulture) +
                      " FEnd: " + pars.EndF.ToString("#.#", CultureInfo.InvariantCulture);
            break;

          case 16:
            info.Alternating = true;
            info.NasalSonority = true;
            info.WhistledSonority = true;
            if ((pars.Alternating == enBoolDC.YES) && ((pars.StartF - pars.EndF) < 11) &&
               ((33 < pars.FME) && (pars.FME < 36) ||
                ((42 < pars.FME) && (pars.FME < 44))) && occurrence(pars, enSpec.BBAR))
              retVal.Add(enSpec.BBAR);
            else if (pars.NasalSonority == enBoolDC.YES)
              step = 17;
            else if (pars.WhistledSonority == enBoolDC.YES)
              step = 18;
            else
              retVal.Add(enSpec.UNKNOWN);
            infoStr = "FME, StartF, EndF, alt, Nasal, Whistled";
            break;

          case 17:
            info.ProgressiveStart = true;
            info.ExplosiveStart = true;
            if ((pars.ProgressiveStart == enBoolDC.YES) && occurrence(pars, enSpec.BBAR))
            {
              retVal.Add(enSpec.BBAR);
            }
            else if (pars.ExplosiveStart == enBoolDC.YES)
            {
              if (occurrence(pars, enSpec.Plecotus))
                retVal.Add(enSpec.Plecotus);
              else
                retVal.Add(enSpec.UNKNOWN);
            }
            infoStr = "Prog: " + pars.ProgressiveStart.ToString() +
                      "Expl:" + pars.ExplosiveStart.ToString();
            break;

          case 18:
            info.HockeyStick = true;
            if (pars.HockeyStick == enBoolDC.YES)
            {
              if (occurrence(pars, enSpec.Pipstrellus))
                retVal.Add(enSpec.Pipstrellus);
              if (occurrence(pars, enSpec.Minopterus))
                retVal.Add(enSpec.Minopterus);
              if (occurrence(pars, enSpec.Hypsugo))
                retVal.Add(enSpec.Hypsugo);
              if (occurrence(pars, enSpec.Vespertilio))
                retVal.Add(enSpec.Vespertilio);
              if (occurrence(pars, enSpec.Nyctalus))
                retVal.Add(enSpec.Nyctalus);
            }
            else
              step = 19;
            infoStr = "Hockestick:" + pars.HockeyStick.ToString();
            break;

          case 19:
            info.FinalWack = true;
            info.AbsenceOfPeek = true;
            info.ExplosiveStart = true;
            //retVal.Add(enSpec.Myotis);
            if ((pars.FinalWack == enBoolDC.YES) && (pars.EndF > 30))
            {
              //acoustic type FW high
            }
            else if ((pars.FinalWack == enBoolDC.YES) && (23 <= pars.EndF) && (pars.EndF < 30))
            {
              //acoustic type FW med
            }
            else if ((pars.FinalWack == enBoolDC.YES) && (pars.EndF < 23))
            {
              //acoustic type FW low
            }
            else if ((pars.ExplosiveStart == enBoolDC.YES) && (pars.EndF > 30))
            {
              //acoustic type ES high
            }
            else if ((pars.ExplosiveStart == enBoolDC.YES) && (23 <= pars.EndF) && (pars.EndF < 30))
            {
              //acoustic type ES med.
            }
            else if ((pars.ExplosiveStart == enBoolDC.YES) && (pars.FinalWack == enBoolDC.YES))
            {
              retVal.Add(enSpec.MBRA);
            }
            else if ((pars.AbsenceOfPeek == enBoolDC.YES) && (pars.EndF > 30))
            {
              //acoustic type abs. high
            }
            else if ((pars.AbsenceOfPeek == enBoolDC.YES) && (23 <= pars.EndF) && (pars.EndF < 30))
            {
              //acoustic type abs. med.
            }
            else if ((pars.AbsenceOfPeek == enBoolDC.YES) && (pars.EndF < 23))
            {
              //acoustic type abs. low
            }
            if (retVal.Count == 0)
              retVal.Add(enSpec.Myotis);
            infoStr = "TODO";
            break;
        }
        steps.Add(lastStep.ToString() + ": " + infoStr);
        lastStep = step;
      }
      return retVal;
    }

    bool occurrence(CallParams pars, enSpec spec)
    {
      return occurrence(spec, pars.Latitude, pars.Longitude);
    }

    /// <summary>
    /// checks if occurrance of the species is possible at the location.
    /// If the point belongs to a region specified in the regions file, the
    /// species is checked against the possible species in the region. If the
    /// location does not belong to a specified region the retunr value is 
    /// always true
    /// </summary>
    /// <param name="spec"></param>
    /// <param name="lat"></param>
    /// <param name="lon"></param>
    /// <returns></returns>
    bool occurrence(enSpec spec, double lat, double lon)
    {
      bool retVal = true;
      ParRegion reg = _batSpecRegions.findRegion(lat, lon);
      if(reg != null)
      {
        string sp = spec.ToString();
        retVal = reg.Species.Contains(sp);
        DebugLog.log("detected location: " + reg.Name, enLogType.INFO);
      }
      else
        DebugLog.log("unrecognized location: " + lat.ToString(CultureInfo.InvariantCulture) + " " + lon.ToString(CultureInfo.InvariantCulture), enLogType.INFO);
      return retVal;
    }
  }
}
