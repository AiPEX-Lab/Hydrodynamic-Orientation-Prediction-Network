#  Takes all objects in a file and gets the projected area at each angle
#  This projected area found, is not very accurate since what i did was 
#  pushed all points on the mesh onto a plane for an angle, then i found 
#  the area of each face using the new point positions.  The reason this 
#  is inaccurate is that the back and front of each of each object is 
#  counted as area. to fix this, i divided it by 2, but it is possible 
#  for the area to overlap mor than twice.

import numpy as np
import math
from pdb import set_trace as t
from os import listdir
from os.path import isfile, join

size = 64
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

def points2area(a,b,c,d=None):  #  Format x=[x,y,z] y=[x,y,z] z=[x,y,z]         turns 3 or 4 points on an obj file into an area
    if d==None:
        d=a
    ab=[b[0]-a[0], b[1]-a[1], b[2]-a[2]]
    ac=[c[0]-a[0], c[1]-a[1], c[2]-a[2]]
    c1=0.5*abs(np.cross(ab,ac))
    r1 = np.linalg.norm(c1)
    db=[b[0]-d[0], b[1]-d[1], b[2]-d[2]]
    dc=[c[0]-d[0], c[1]-d[1], c[2]-d[2]]
    c2=0.5*abs(np.cross(db,dc))
    r2 = np.linalg.norm(c2)
    result = r1 + r2
    return result

def pointplane2point(x,y,z,u,v,w):  #  point=[x,y,z]  plane=[u,v,w] this is angle cartesian          this takeas a point and puts it onto a given plane
    x=float(x)
    y=float(y)
    z=float(z)
    u=1/u if u!=0 else 1000
    v=1/v if v!=0 else 1000
    w=1/w if w!=0 else 1000
    s=(-x-y-x)/(u+v+w)
    a=x-s*u
    b=y-s*v
    c=z-s*w
    return a, b, c

def area(link):  #  take obj file and gives area for all given angles
    with open(link) as f:
        content = f.readlines()
    
    points=[]
    faces=[]
    for i in content:  #  Get points and faces out of obj file
        if i[0]=='f':
            lst=[]
            while ' ' in i:
                s = i.find(' ')
                i=i[s+1:]
                s = i.find(' ')
                x = i[:s]
                if '/' in x:
                    s=x.find('/')
                    x=x[:s]
                if len(x)!=0:
                    lst.append(int(x))
            faces.append(lst)
        elif i[0]=='v' and i[1]!='t' and i[1]!='n':
            lst=[]
            while ' ' in i:
                s = i.find(' ')
                i=i[s+1:]
                s = i.find(' ')
                x = i[:s]
                if '/' in x:
                    s=x.find('/')
                    x=x[:s]
                lst.append(float(x))
            points.append(lst)
    
    fib=fibonacci_sphere(size)
    area=[]
    for j in fib:  #  for each angle
        npoints=[]  #  points put onto place
        u=j[0]
        v=j[1]
        w=j[2]
        for k in points:  #  for each point in mesh
            a,b,c=pointplane2point(k[0],k[1],k[2],u,v,w)
            npoints.append([a,b,c])
        sum=0
        for i in faces:
            if len(i)==3:
                sum+=points2area(npoints[i[0]-1],npoints[i[1]-1],npoints[i[2]-1])
            elif len(i)==4:
                try:
                    sum+=points2area(npoints[i[0]-1],npoints[i[1]-1],npoints[i[2]-1],npoints[i[3]-1])
                except:
                    t()
        area.append(sum/2)
    
    return area
        
#path = r'C:\__main__\R - DATA\Untitled Folder\Seans_files\Discarded\areas.txt'  #  path for text file to write to
#onlyfiles = [f for f in listdir(r'C:\__main__\R - DATA\Untitled Folder\Seans_files\Meshes')]  #  path for objects
path = 'areas.txt'
onlyfiles = [f for f in listdir()]

areas=[]
count=0
for i in onlyfiles:  #  for each obj file
    if '.obj' in i:
        f=open(path, 'a+')
        print(i, '           ', count)
        var = area(i)
        areas.append(var)
        wr=i+' '+str(var)+'\n'
        f.write(wr)
        f.close()
        count+=1


for j in areas:
    print(j)
