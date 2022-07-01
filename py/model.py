import numpy as np
import os
import shutil
import tensorflow as tf
import glob

from sklearn.metrics import accuracy_score, confusion_matrix


def clean_logs(data_dir):
    logs_dir = os.path.join(data_dir, "logs")
    shutil.rmtree(logs_dir, ignore_errors=True)
    return logs_dir


def readData(dirName):
    """
    read data from data files
    
    Returns:
    train_x - np.array (3,330) train data
    train_y - np.array (1,18) train result
    dev_X,
    dev_y,
    test_x,
    test_y
    """
    train_x = np.load(dirName + "Xtrain000.npy")
    np.swapaxes(train_x,1,2)
    train_y = np.load(dirName + "Ytrain000.npy")
    print("x train shape:", train_x.shape)
    print("y train shape:", train_y.shape)
    dev_x = np.load(dirName + "Xdev000.npy")
    np.swapaxes(dev_x,1,2)
    dev_y = np.load(dirName + "Ydev000.npy")
    test_x = np.load(dirName + "Xtest000.npy")
    np.swapaxes(test_x,1,2)    
    test_y = np.load(dirName + "Ytest000.npy")
    return train_x, train_y,dev_x, dev_y, test_x, test_y


def tf_data_generator(files_x, files_y):
    i = 0
    while True:
#        print("gen 1 call")
        if i >= len(files_x):  
            i = 0
        else:
            filex = files_x[i]
            filey = files_y[i]
            train_x = np.load(filex)
            np.swapaxes(train_x,1,2)
            train_y = np.load(filey)
            train = tf.data.Dataset.from_tensor_slices((train_x, train_y))
        
            # Create tensorflow dataset so that we can use `map` function that can do parallel computation.
#            data_ds = tf.data.Dataset.from_tensor_slices(data)
#            data_ds = data_ds.batch(batch_size = first_dim).map(data_transformation_func,
#                                                                num_parallel_calls = tf.data.experimental.AUTOTUNE)
            # Convert the dataset to a generator and subsequently to numpy array
#            data_ds = tfds.as_numpy(data_ds)   # This is where tensorflow-datasets library is used.
#            data = np.asarray([data for data in data_ds]).reshape(first_dim,31,33,1)
            
            yield train
            i = i + 1
    
def readDataset(dirName):
    """
    read data from data files
    
    Returns:
    train tf.Dataset :np.array (3,330) train data, np.array (1,18) train result
    dev
    test
    """
    train_x = np.load(dirName + "Xtrain000.npy")
    rows = train_x.shape[1]
    np.swapaxes(train_x,1,2)
    m = train_x.shape[0]
    cols = train_x.shape[2]
    train_y = np.load(dirName + "Ytrain000.npy")
    classes = train_y.shape[1]
    files_x = glob.glob(dirName +"/Xtrain*.npy")
    files_y = glob.glob(dirName +"/Ytrain*.npy")
    train = tf.data.Dataset.from_tensor_slices((train_x, train_y))
#    train = tf.data.Dataset.from_generator(tf_data_generator, args= [files_x, files_y],
#                                            output_types=(tf.float64, tf.float64),
#                                            output_shapes = ((rows,cols),(classes,)))                                           

#output_signature=(
#                                                tf.TensorSpec(shape=(rows, cols), dtype=tf.float64),
#                                                tf.TensorSpec(shape=(classes,), dtype=tf.float64)
#                                                ))
    dev_x = np.load(dirName + "Xdev000.npy")
    np.swapaxes(dev_x,1,2)
    dev_y = np.load(dirName + "Ydev000.npy")
    dev = tf.data.Dataset.from_tensor_slices((dev_x, dev_y))
    test_x = np.load(dirName + "Xtest000.npy")
    np.swapaxes(test_x,1,2)    
    test_y = np.load(dirName + "Ytest000.npy")
    test = tf.data.Dataset.from_tensor_slices((test_x, test_y))
    return train, dev, test



def flatModel(rows, timeSteps, classes):
    model = tf.keras.Sequential()
    model.add(tf.keras.Input(shape=(rows, timeSteps)))
    model.add(tf.keras.layers.Flatten())
    model.add(tf.keras.layers.Dense(256,  activation="relu"))
    model.add(tf.keras.layers.Dense(128,  activation="relu"))
    model.add(tf.keras.layers.Dense(64,  activation="relu"))
    model.add(tf.keras.layers.Dense(classes, activation="softmax"))
    return model

def rnnModel(rows, timeSteps, classes):
    model = tf.keras.Sequential()
    model.add(tf.keras.layers.GRU(64, input_shape=(rows, timeSteps), return_sequences=True))
    model.add(tf.keras.layers.Flatten())
    model.add(tf.keras.layers.BatchNormalization())
    model.add(tf.keras.layers.Dense(256,  activation="relu"))
    model.add(tf.keras.layers.Dense(64,  activation="relu"))
    model.add(tf.keras.layers.Dense(classes, activation="softmax"))
    return model

##### main ######

print("#####################")
# set random seed
tf.random.set_seed(42)

# clean up log area
#data_dir = "./data"
#logs_dir = clean_logs(data_dir)

# download and read data into data structures

batch_size = 64
#train_x, train_y, dev_x, dev_y, test_x, test_y = readData("C:/Users/chrmu/prj/BatInspector/mod/trn/fft/")
print("start reading data set")
train, dev, test = readDataset("C:/Users/chrmu/prj/BatInspector/mod/trn/")
it = iter(dev)
elem = next(it)
rows = elem[0].shape[0]
timeSteps = elem[0].shape[1]
classes = elem[1].shape[0]

print ("rows, timeSteps, classes:",rows, timeSteps, classes)
    
# define model
#model = rnnModel(rows, timeSteps)
model = flatModel(rows, timeSteps, classes)
print(model.summary())

opt = tf.keras.optimizers.Adam(learning_rate=0.001)
# compile
model.compile(
    loss="categorical_crossentropy",
#    loss="sparse_categorical_crossentropy",
    optimizer=opt,   #"adam", 
    metrics=["accuracy"]
)

train = train.batch(batch_size, drop_remainder=True)
dev = dev.batch(batch_size, drop_remainder=True)
#model.fit(train_x, train_y, validation_data=(dev_x, dev_y), batch_size=32, epochs=10)
model.fit(train, validation_data=dev, batch_size=batch_size, epochs=30)

# train
#best_model_file = os.path.join(data_dir, "best_modl.h5")
#checkpoint = tf.keras.callbacks.ModelCheckpoint(best_model_file,
#    save_weights_only=True,
#    save_best_only=True)
#tensorboard = tf.keras.callbacks.TensorBoard(log_dir=logs_dir)
#num_epochs = 10
#history = model.fit(train_dataset, epochs=num_epochs, 
#    validation_data=val_dataset,
#    callbacks=[checkpoint, tensorboard])

# evaluate with test set
#best_model = model(vocab_size+1, max_seqlen)
#best_model.build(input_shape=(batch_size, max_seqlen))
#best_model.load_weights(best_model_file)
#best_model.compile(
#    loss="binary_crossentropy",
#    optimizer="adam", 
#    metrics=["accuracy"]
#)

#test_loss, test_acc = best_model.evaluate(test_dataset)
#print("test loss: {:.3f}, test accuracy: {:.3f}".format(test_loss, test_acc))

# predict on batches
#labels, predictions = [], []
#idx2word[0] = "PAD"
#is_first_batch = True
#for test_batch in test_dataset:
#    inputs_b, labels_b = test_batch
#    pred_batch = best_model.predict(inputs_b)
#    predictions.extend([(1 if p > 0.5 else 0) for p in pred_batch])
#    labels.extend([l for l in labels_b])
#    if is_first_batch:
#        for rid in range(inputs_b.shape[0]):
#            words = [idx2word[idx] for idx in inputs_b[rid].numpy()]
#            words = [w for w in words if w != "PAD"]
#            sentence = " ".join(words)
#            print("{:d}\t{:d}\t{:s}".format(labels[rid], predictions[rid], sentence))
#        is_first_batch = False

#print("accuracy score: {:.3f}".format(accuracy_score(labels, predictions)))
#print("confusion matrix")
#print(confusion_matrix(labels, predictions))
