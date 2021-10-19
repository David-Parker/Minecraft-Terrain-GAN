# Minecraft Terrain GAN  

Minecraft has been applauded for its terrain generation for years. Traditionally, terrain was created in Minecraft using Ken Perlin's noise function.
This creates realistic and auto-generated mountains and valleys. However, this approach suffers from not being able to create interesting structures on Earth created from erosion, such as arches and cliffs.  

Coming up with a mathematical function that can describe these structures has evaded researchers and engineers in the field for years. 
In the last couple of years, research into modeling functions from data, rather than first principles, has exploded. 
Many new variants of Neural Networks have come into existence. One such network architecture, invented by Ian Goodfellow, is known as a Generative Adversarial Network. 
The basic idea is to train two networks simultaneously in a minimax optimization game. One network will try to generate new versions of the input data, and the other network will try to decide if
 the version came from the actual dataset or the generated dataset. As these two networks play this minimax "game", the generator gets very good at creating new copies or clones of the input data.  

We plan to use a GAN and train it on real world terrain data and then use it to generate new terrain datasets and visualize them in a Minecraft voxel-like world.

![GAN Generated Terrain:](https://i.imgur.com/woQaggq.jpg)

Video:
[![Minecraft GAN](https://img.youtube.com/vi/e-n1XI-Wb5U/0.jpg)](https://www.youtube.com/watch?v=e-n1XI-Wb5U "Minecraft Terrain Generator GAN")
