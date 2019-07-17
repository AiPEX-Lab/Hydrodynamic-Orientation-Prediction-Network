#  This code shows the angle distribution and also outputs the angles
#  to use in the drag simulation (we used unity)

import numpy as np
import math as m
from mpl_toolkits.mplot3d import Axes3D
import matplotlib.pyplot as plt
from itertools import product, combinations
from pdb import set_trace as t
import random

num=64  #  number of angles to use

def cart2sph(x, y, z):  #  cartesian coordinates to spherical coordinates
    hxy = np.hypot(x, y)
    r = np.hypot(hxy, z)
    el = np.arctan2(z, hxy)
    az = np.arctan2(y, x)
    return az, el, r
    
def sph2cart(az, el, r):  #  Spherical coordinates to cartesian coordinates
    rcos_theta = r * np.cos(el)
    x = rcos_theta * np.cos(az)
    y = rcos_theta * np.sin(az)
    z = r * np.sin(el)
    return x, y, z

def fibonacci_sphere(samples=1,randomize=False):  #  input number of points, output [x,y,z] coordinates of equally spaced points on a sphere
    rnd = 1.
    if randomize:
        rnd = random.random() * samples
    points = []
    offset = 2./samples
    increment = m.pi * (3. - m.sqrt(5.));
    for i in range(samples):
        y = ((i * offset) - 1) + (offset / 2);
        r = m.sqrt(1 - pow(y,2))
        phi = ((i + rnd) % samples) * increment
        x = m.cos(phi) * r
        z = m.sin(phi) * r
        points.append([x,y,z])
    return points
  
if True: #  gets x,y,z coordinates out of fibonacci_sphere function
    lst = fibonacci_sphere(num)
    x,y,z=zip(*lst)

if True:  #  plot
    fig = plt.figure()
    ax = fig.gca(projection='3d')
    ax.scatter(x,y,z, color='k')
    plt.show()
    
if False:  #  test plot
    fig = plt.figure()
    ax = fig.gca(projection='3d')
    lst2=[]
    for i in lst:
        e,f,g = cart2sph(i[0],i[1],i[2])
        lst2.append(sph2cart(e,f,g))
    X,Y,Z=zip(*lst2)
    ax.scatter(X,Y,Z, color='k')
    plt.show()

phi=[]
theta=[]
for i in range(len(lst)):  #  changes given cartesion coordinates in spherical form
    p,t,r = cart2sph(x[i],y[i],z[i])
    phi.append(p)
    theta.append(t)
    
for i in theta:  #  gives theta values, replace with phi for phi values
    print(i)
    
