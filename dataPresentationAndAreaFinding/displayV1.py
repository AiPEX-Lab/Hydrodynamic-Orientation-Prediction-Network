#  Displays final results of training


from mpl_toolkits.mplot3d import Axes3D
import matplotlib.pyplot as plt
import numpy as np
from itertools import product, combinations
import math
from pdb import set_trace as t
import random

if True:  #  Create list
    size=100
    l=[]  #  Make l the data from the algorithm
    for i in range(size):
        l.append(0.1)
        #l.append(random.uniform(0, 1))

def fibonacci_sphere(samples=1,randomize=False):  #  input number of points, output [x,y,z] coordinates of equally spaced points on a sphere
    rnd = 1.
    if randomize:
        rnd = random.random() * samples
    points = []
    offset = 2./samples
    increment = math.pi * (3. - math.sqrt(5.));
    for i in range(samples):
        y = ((i * offset) - 1) + (offset / 2);
        r = math.sqrt(1 - pow(y,2))
        phi = ((i + rnd) % samples) * increment
        x = math.cos(phi) * r
        z = math.sin(phi) * r
        points.append([x,y,z])
    return points

if True:  #  Get angles
    points = fibonacci_sphere(size)
    phi = []
    theta = []
    for i in points:
        phi.append(math.degrees(math.atan(i[1]/i[0])))
        if i[2]==0:
            theta.append(0)
        else:
            a=1
            b=i[2]
            c=a/b
            e=math.atan(c)
            f=math.degrees(e)
            if f < 0:
                f+=90
            elif f > 0:
                f-=90
            else:
                print('error')
            theta.append(f)
            
if False:  #  3D bar graph
    # setup the figure and axes
    fig = plt.figure()
    ax1 = fig.add_subplot(111, projection='3d')
    
    # fake data
    _x = np.arange(math.sqrt(size))
    _y = np.arange(math.sqrt(size))
    _xx, _yy = np.meshgrid(_x, _y)
    x, y = _xx.ravel(), _yy.ravel()
    
    top = l
    bottom = np.zeros_like(top)
    width = depth = 0.5
    
    ax1.bar3d(x, y, bottom, width, depth, top, shade=True)
    ax1.set_title('Shaded')
    
    plt.show()

if True:  #  3D spherical plot
    
    fig = plt.figure()
    ax = fig.gca(projection='3d')
    
    # draw sphere
    u, v = np.mgrid[0:2*np.pi:20j, 0:np.pi:10j]
    x = np.cos(u)*np.sin(v)
    y = np.sin(u)*np.sin(v)
    z = np.cos(v)
    ax.plot_surface(x, y, z, color='#79a3e8')
    
    # draw points
    lst=fibonacci_sphere(size)
    X=[]
    Y=[]
    Z=[]
    X, Y, Z = zip(*lst)
    
    X = [a*(b+1) for a,b in zip(X,l)]
    Y = [a*(b+1) for a,b in zip(Y,l)]
    Z = [a*(b+1) for a,b in zip(Z,l)]
    
    ax.scatter(X,Y,Z, color='k')
    
    plt.show()

