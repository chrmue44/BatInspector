################################################################################
# bulk processing of recorded files                 
# arg1: name of the directory with the *.wav files
# arg2: name of result file
################################################################################

library(bioacoustics)
library(tools)
library(randomForest)
library(rstudioapi)
library(stringr)

########################################
# Let's consolidate all the output data:

check_species <- function(fname, species)
{
  len = length(species[[1]])
  for (i in 1:len) {
    spec = species[i,1]
    if (str_detect(fname, spec)) {
      return (spec)   
    }
  }
  return("----")
}


#########################################
# Main
############################################
args <- commandArgs(trailingOnly=TRUE)
if (length(args)<2) {
   dataDir <-  "C:/Users/chrmu/bat/train"
   resultFile <-"C:/Users/chrmu/bat/train/calls.csv"
   specFile  <-"C:/Users/chrmu/bat/train/species.csv"
} else {
  dataDir <-   args[1]
  resultFile <- args[2]
  specFile <- args[3]
}

species <- read.csv(file = specFile)
#setwd("~/prj/bioacoustics")
setwd("C:/Users/chrmu/prj/BatInspector/R")
filesToTest <- dir(dataDir, recursive = TRUE, full.names = TRUE, pattern = "[.]wav$")
calls = 0
cat("name", "nr","spec","sampleRate","FileLen","freq_max_amp","freq_min","freq_max","freq_knee","duration", "start", "snr","spec_man","comment","----",file=resultFile, sep = ";")
for (sp in species) {
  cat("", sp, file=resultFile, sep = ";", append=TRUE)
}
cat("", "\n", file=resultFile, sep = ";", append=TRUE)

  for (fileToTest in filesToTest) {
    actSpec = check_species(fileToTest, species)
    tryCatch( {    
      print(fileToTest)
      TDs <- setNames(
      lapply(
        fileToTest,
        threshold_detection,
        threshold = 5, 
        min_dur = 1.5, 
        max_dur = 80, 
        min_TBE = 30, 
        max_TBE = Inf,
        LPF = 120000, 
        HPF = 10000, 
        FFT_size = 256, 
        start_thr = 30, 
        end_thr = 35, 
        SNR_thr = 5, 
        angle_thr = 125, 
        duration_thr = 80, 
        spectro_dir = NULL,
        NWS = 100, 
        KPE = 0.00001, 
        time_scale = 2, 
        EDG = 0.996
        ),
      basename(file_path_sans_ext(fileToTest))
      )

# Keep only files with data in it
      TDs <- TDs[lapply(TDs, function(x) length(x$data)) > 0]

# Keep the extracted feature and merge in a single data frame for further analysis
      EvDat <- do.call("rbind", c(lapply(TDs, function(x) x$data$event_data), list(stringsAsFactors = FALSE)))
      nrOfObjects <- length(TDs)
      if (nrOfObjects == 0) {
        file.remove(fileToTest)
        xmlFile <- paste(file_path_sans_ext(fileToTest), ".xml",sep ="")
        file.remove(xmlFile)
        msg <- paste("removed ", fileToTest, sep = " ")
        print(msg)   
      } else {
        nrOfObjects <- length(EvDat$filename)
        for(i in 1:nrOfObjects) {
          cat(fileToTest, i,actSpec,"",file=resultFile, sep = ";", append=TRUE)
          cat(383500, 3.001, file=resultFile,"", sep = ";", append=TRUE)
          cat(EvDat$freq_max_amp[i],EvDat$freq_min[i],EvDat$freq_max[i],"",file=resultFile, sep = ";", append=TRUE)
          cat(EvDat$freq_knee[i], EvDat$duration[i], EvDat$starting_time[i], EvDat$snr[i],"\n",file=resultFile, sep = ";", append=TRUE)
          calls <- calls + 1
        }
      }
      gc()
    })     
  }
  msg <- paste("nr of detected training samples:", calls, sep = " ")
  print(msg)

