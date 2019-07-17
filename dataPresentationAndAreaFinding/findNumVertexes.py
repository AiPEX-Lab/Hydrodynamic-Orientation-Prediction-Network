#  This file finds the maximum and minimum number of vertexes in the dataset of objects
#  this is then used to tell how many null items to add to the list

from os import listdir
from pdb import set_trace as t

path = 'points.txt'  #  write to
onlyfiles = [f for f in listdir(r'objects')]  #  read from

if True:
    points=[]
    count=0
    long=0
    short=1000000
    for i in onlyfiles:
        if '.obj' in i:
            f=open(path, 'a+')
            print(i, '          ', count, )
            count+=1
            with open(i) as g:
                contents=g.readlines()
            lst=[]
            for j in contents:
                if j[0]=='v' and j[1]!='t' and j[1]!='n':
                    lst1=[]
                    while ' ' in j:
                        s = j.find(' ')
                        j=j[s+1:]
                        s = j.find( ' ')
                        x = j[:s]
                        lst1.append(x)
                    lst.append(lst1)
            length=len(lst)
            if length > long:
                long=length
            if length < short:
                short=length
            string=str(lst)+'\n'
            f.write(string)
            f.close()
            
            
print(long)
print(short)