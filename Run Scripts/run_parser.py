#Get data from each object run and create master data file for exporting

import os
import json

RUNPATH = os.getenv('APPDATA') + '/DARPA_RUNS'

def check_app_dir():
	#are there files to parse?
	return os.path.exists(RUNPATH)

def map_run_dir():
	#get array of files to parse
	return os.listdir(RUNPATH)
	
def read_files(parsed_files):
	parsed_objs = []
	#parse each file to object
	for fileName in parsed_files:
		#skip the previously existing dictionary
		if fileName == "object_dict.txt":
			continue
		filePath = RUNPATH + "/" + fileName
		f = open(filePath, "r")
		#read drags of object
		drags = f.read().splitlines()
		#create run object
		_obj = {
			"name" : fileName.replace(".txt", ""),
			"drags" : drags
		}
		#add to cumulative object
		parsed_objs.append(_obj)
	#create dict object
	cumulative_obj = {
		"objs" : parsed_objs
	}
	return json.dumps(cumulative_obj, indent=4)
	
def write_cumulative_file(json):
	#write dict object
	cumulative_file_path = RUNPATH + "/object_dict.txt"
	f = open(cumulative_file_path, "w")
	f.write(json)
	f.close()
	
if __name__ == '__main__':
	#check directory exists
	if check_app_dir():
		#get mapped ran files
		parsed_files = map_run_dir()
		#get json of cumulative object
		cumulative_json = read_files(parsed_files)
		#write dictionary file
		write_cumulative_file(cumulative_json)
	else:
		print("Could not read run files")