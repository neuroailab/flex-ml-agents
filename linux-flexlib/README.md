# How to build and run a FleX ML Agents binary for Linux

***Important: FleX only works with NVidia GPUs and CUDA 8.0***

To use Flex on Linux you need to create a dynamic library that links FleX against your CUDA installation. We have already pregenerated a shared library called `libflexUtils.so` which assumes your CUDA `libcudart.so` is located at `/usr/local/cuda/lib64/`. 

If your CUDA installation is **not** located in this folder you need to locate `libcudart.so` on your machine and create a dynamic library file for Unity that links against it yourself.
Using the files in this folder `linux-flexlib`, execute the following command to link these objects and libraries and CUDA (libcudart.so) into a shared libarary called `libflexUtils`:

```bash
g++ -shared -o libflexUtils.so -Wl,--whole-archive NvFlex.o NvFlexReleaseCUDA_x64.a NvFlexExtReleaseCUDA_x64.a /usr/local/cuda/lib64/libcudart.so -Wl,--no-whole-archive
```

Copy the created libflexUtils.so into `UnitySDK/Assets/GameWorks/Flex/Engine/Native/Plugins/Linux` and overwrite the existing file.

Once this is done you can build a Linux binary of FleX ML Agents in the Unity Editor on a Windows machine. Therefore, open the **UnitySDK** project and select **File&rarr;Build Settings...**. Change the `Target Platform` to `Linux` and the `Architecture` to `x86_64`. Enable `Headless Mode` if your environment does not render out any images. Click on `Build` and select where you would like to store your binary. Press `Save` and you are done and your binary will have been build.

Without `Headless Mode`, that means if you want to render images on a cluster without a display, you will need to have **X Server** running on your cluster machine. To start up **X** execute the following commands:

```bash
sudo service lightdm stop
sudo nvidia-xconfig -a --use-display-device=None --virtual=1280x1024
sudo /usr/bin/X :0
export DISPLAY=":0"
```

You can control which GPU is used for rendering on **X** by setting `export DISPLAY=":0.0"` for the first GPU, `export DISPLAY=":0.1"` for the second GPU and so on. Use `export CUDA_VISIBLE_DEVICES="0,1,2,3"` to control which GPUs are used for CUDA computations, that means FleX as well as Tensorflow or PyTorch.

Once you have completed this setup, you can run flex-ml-agents on your Linux cluster, as you would normally do:
```bash
mlagents-learn config/trainer_config.yaml --run-id=flextest --train --env=<YOUR_ENVIRONMENT>.x86_64 --no-graphics
```
