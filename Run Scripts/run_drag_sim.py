#Get objects to parse from directory, add to threadpool queue, and run unity sim for each

import multiprocessing as mp
import subprocess as sp
import os

RUNPATH = os.getenv('APPDATA') + '/DARPA_RUNS/'
DATAPATH = os.getenv('APPDATA') + '/DARPA_DATA/'
UNITYSIM = 'objdragsim.exe'
UNITYSIMPATH = './unity-sim/'

def _EvaluationFunction(name):
	uuids_str = name
	if not os.path.exists(RUNPATH):
		os.makedirs(RUNPATH)
	RunUnitySim(uuids_str)
	
def RunUnitySim(uuids, headless = True):
	#launch the unity sim
	run_str = UNITYSIMPATH + UNITYSIM + ' '+DATAPATH + uuids + ' -silent-crashes -quit'
	print(run_str)
	if headless:
		#run sim in headless mode
		run_str += ' -batchmode -nographics'
	try:
		#attempt to run the unity sim with args
		p = sp.run(run_str)
		print(p)
	except Exception as e:
		print(e)

def map_obj_dir():
	#get all files in data directory
	return os.listdir(DATAPATH)

def check_app_dir():
	#check dependent directories
	_runPath = os.path.exists(RUNPATH)
	_argPath = os.path.exists(DATAPATH)
	#check unity sim is where it should be
	_simPath = True#os.path.exists(UNITYSIMPATH + UNITYSIM)
	return _runPath and _argPath and _simPath

if __name__ == '__main__':
	#check dependent directories
	if check_app_dir():
		#map object directory for data file names
		object_files = map_obj_dir()
		#create thread pool for processor core count
		pool = mp.Pool(mp.cpu_count())
		#add object file processing to the thread pool queue
		pool.map(_EvaluationFunction, object_files)
		pool.close()
		pool.join()
	else:
		print('Missing dependent directories')
	
