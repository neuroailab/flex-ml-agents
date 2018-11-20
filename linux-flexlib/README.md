To use Flex on Linux link these libraries and CUDA (libcudart.so) into a shared libarary with the following command:
```
g++ -shared -o libFlexUtils.so -Wl,--whole-archive NvFlex.o NvFlexReleaseCUDA_x64.a NvFlexExtReleaseCUDA_x64.a /usr/local/cuda/lib64/libcudart.so -Wl,--no-whole-archive
```
and copy libFlexUtils.so into Assets/GameWorks/Flex/Engine/Native/Plugins/Linux.
