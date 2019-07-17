#  This code displays the results in an astetically pleasing way

from mpl_toolkits.mplot3d import Axes3D
import matplotlib.pyplot as plt
import numpy as np
from itertools import product, combinations
import math
from pdb import set_trace as t
import random
import os

num='099_coll'

if True:  #  Create list
    p=r'.\orientation_results.txt'
    h=open(p, 'r')
    contents=h.readlines()
    for i in contents:
        i=eval(i[:-1])
        s=num
        if i[0]==s:
            l=i[1]
            break
    size=25
    #for i in range(size):
        #l.append(0.1)
        #l.append(random.uniform(0, 1))
    minIndex = l.index(min(l))
     

def fibonacci_sphere(samples=1,randomize=False):
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

def cart2sph(x, y, z):
    hxy = np.hypot(x, y)
    r = np.hypot(hxy, z)
    el = np.arctan2(z, hxy)
    az = np.arctan2(y, x)
    return az, el, r

def sph2cart(az, el, r):
    rcos_theta = r * np.cos(el)
    x = rcos_theta * np.cos(az)
    y = rcos_theta * np.sin(az)
    z = r * np.sin(el)
    return x, y, z

if True:  #  Get angles
    points = fibonacci_sphere(size)
    x,y,z=zip(*points)
    theta=[]
    phi=[]
    for i in points:
        a,b,c = cart2sph(i[0], i[1], i[2])
        theta.append(a)
        phi.append(b)
    #for i in phi:
    #    print(i)

if True:  #  3D bar graph
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
    if True:
        u, v = np.mgrid[0:2*np.pi:20j, 0:np.pi:10j]
        x = np.multiply(np.cos(u)*np.sin(v),0.5)
        y = np.multiply(np.sin(u)*np.sin(v),0.5)
        z = np.multiply(np.cos(v),0.5)
        ax.plot_surface(x, y, z, color='#79a3e8')
    else:
        path=r'C:\__main__\R - DATA\Untitled Folder\Seans_files\objects\099_coll.obj'
        f=open(path, 'r')
        contents=f.readlines()
        lst2=[]
        for i in contents:
            if i[0]=='v' and i[1]!='t' and i[1]!='n':
                lst1=[]
                while ' ' in i:
                    s = i.find(' ')
                    i=i[s+1:]
                    s = i.find( ' ')
                    x = i[:s]
                    lst1.append(x)
                lst2.append(lst1)
        x,y,z=zip(*lst2)
        x=list(x)
        y=list(y)
        z=list(z)
        for i in range(len(x)):
            x[i]=float(x[i])
        for i in range(len(y)):
            y[i]=float(y[i])
        for i in range(len(z)):
            z[i]=float(z[i])
        m=10
        for i in range(len(x)):
            x[i]=x[i]/m
        for i in range(len(y)):
            y[i]=y[i]/m
        for i in range(len(z)):
            z[i]=z[i]/m
        ax.scatter(x,y,z,color='#79a3e8')
    
    # draw points
    lst=fibonacci_sphere(size)
    
    X, Y, Z = zip(*points)
    color=np.abs(l)
    ma=max(l)
    l2=[]
    for i in range(len(l)):
        l2.append(int(255*(l[i]/ma)))
    hex1=[]
    for i in l2:
        hx=str(hex(i))[2:]
        hex1.append(hx)
    c2=[]
    for i in range(len(hex1)):
        if i==minIndex:
            c1='#FF0000'
        else:
            p1='#'
            p2=hex1[i]
            p3=hex1[i]
            p4='00'
            c1=p1+p2+p3+p4
        c2.append(c1)
    #X = [a*(b+0.5) for a,b in zip(X,l)]
    #Y = [a*(b+0.5) for a,b in zip(Y,l)]
    #Z = [a*(b+0.5) for a,b in zip(Z,l)]
    ax.scatter(X,Y,Z,c=c2,s=500, alpha=0.5)
    
    for angle in range(0, 360):
        ax.view_init(30, angle)
        plt.draw()
        plt.pause(.001)

if False:  #  3d spherical coordinates check
    
    fig = plt.figure()
    ax = fig.gca(projection='3d')
    
    # draw points
    Xt=[]
    Yt=[]
    Zt=[]
    for i in range(len(lst)):
        x,y,z=sph2cart(theta[i], phi[i], 1)
        Xt.append(x)
        Yt.append(y)
        Zt.append(z)
    
    ax.scatter(Xt,Yt,Zt, color='k')
    plt.show()


x,y,z=lst[minIndex][0],lst[minIndex][1],lst[minIndex][2]
a,b,c=cart2sph(x,y,z)
a=math.degrees(a)
b=math.degrees(b)
print('phi (x)', a, 'theta (y)', b)





