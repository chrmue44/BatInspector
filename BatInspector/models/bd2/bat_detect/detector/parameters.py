import numpy as np
import os
import datetime


def mk_dir(path):
    if not os.path.isdir(path):
        os.makedirs(path)
    
    
def get_params(make_dirs=False, exps_dir='../../experiments/'):
    params = {}

    params['model_name'] = 'Net2DFast' # Net2DFast, Net2DSkip, Net2DSimple, Net2DSkipDS, Net2DRN
    params['num_filters'] =  128

    now_str = datetime.datetime.now().strftime("%Y_%m_%d__%H_%M_%S")
    model_name                = now_str + '.pth.tar'
    params['experiment']      = os.path.join(exps_dir, now_str, '')
    params['model_file_name'] = os.path.join(params['experiment'], model_name)
    params['op_im_dir']       = os.path.join(params['experiment'], 'op_ims', '')
    params['op_im_dir_test']  = os.path.join(params['experiment'], 'op_ims_test', '')
    #params['notes']           = ''  # can save notes about an experiment here


    # spec parameters
    params['target_samp_rate'] = 256000        # resamples all audio so that it is at this rate
    params['fft_win_length'] = 512 / 256000.0  # in milliseconds, amount of time per stft time step
    params['fft_overlap']    = 0.75            # stft window overlap

    params['max_freq'] = 120000       # in Hz, everything above this will be discarded
    params['min_freq'] = 10000        # in Hz, everything below this will be discarded

    params['resize_factor'] = 0.5     # resize so the spectrogram at the input of the network
    params['spec_height'] = 256       # units are number of frequency bins (before resizing is performed)
    params['spec_train_width'] = 512  # units are number of time steps (before resizing is performed)
    params['spec_divide_factor'] = 32 # spectrogram should be divisible by this amount in width and height

    # spec processing params
    params['denoise_spec_avg'] = True  # removes the mean for each frequency band
    params['scale_raw_audio'] = False  # scales the raw audio to [-1, 1]
    params['max_scale_spec'] = False   # scales the spectrogram so that it is max 1
    params['spec_scale'] = 'pcen'      # 'log', 'pcen', 'none'

    # detection params
    params['detection_overlap'] = 0.01   # has to be within this number of ms to count as detection
    params['ignore_start_end'] = 0.01    # if start of GT calls are within this time from the start/end of file ignore
    params['detection_threshold'] = 0.01 # the smaller this is the better the recall will be
    params['nms_kernel_size'] = 9
    params['nms_top_k_per_sec'] = 200    # keep top K highest predictions per second of audio
    params['target_sigma'] = 2.0

    # augmentation params
    params['aug_prob'] = 0.20               # augmentations will be performed with this probability
    params['augment_at_train'] = True
    params['augment_at_train_combine'] = True
    params['echo_max_delay'] = 0.005        # simulate echo by adding copy of raw audio
    params['stretch_squeeze_delta'] = 0.04  # stretch or squeeze spec
    params['mask_max_time_perc'] = 0.05     # max mask size - here percentage, not ideal
    params['mask_max_freq_perc'] = 0.10     # max mask size - here percentage, not ideal
    params['spec_amp_scaling']   = 2.0      # multiply the "volume" by 0:X times current amount
    params['aug_sampling_rates'] = [220500, 256000, 300000, 312500, 384000, 441000, 500000]

    # loss params
    params['train_loss'] = 'focal'         # mse or focal
    params['det_loss_weight'] = 1.0        # weight for the detection part of the loss
    params['size_loss_weight'] = 0.1       # weight for the bbox size loss
    params['class_loss_weight'] = 2.0      # weight for the classification loss
    params['individual_loss_weight'] = 0.0 # not used
    if params['individual_loss_weight'] == 0.0:
        params['emb_dim'] = 0              # number of dimensions used for individual id embedding
    else:
        params['emb_dim'] = 3

    # train params
    params['lr'] = 0.001
    params['batch_size'] = 8
    params['num_workers'] = 4
    params['num_epochs'] = 200
    params['num_eval_epochs'] = 5  # run evaluation every X epochs
    params['device'] = 'cuda'
    params['save_test_image_during_train'] = False
    params['save_test_image_after_train'] = True

    params['convert_to_genus'] = False
    params['genus_mapping'] = []
    params['class_names'] = []
    params['classes_to_ignore'] = ['', ' ', 'Unknown', 'Not Bat']
    params['generic_class'] = ['Bat']
    params['events_of_interest'] = ['Echolocation']  # will ignore all other types of events e.g. social calls

    # the classes in this list are standardized during training so that the same low and high freq are used
    params['standardize_classs_names'] = []

    # create directories
    if make_dirs:
        print('Model name : ' + params['model_name'])
        print('Model file : ' + params['model_file_name'])
        print('Experiment : ' + params['experiment'])

        mk_dir(params['experiment'])
        if params['save_test_image_during_train']:
            mk_dir(params['op_im_dir'])
        if params['save_test_image_after_train']:
            mk_dir(params['op_im_dir_test'])
        mk_dir(os.path.dirname(params['model_file_name']))

    return params
