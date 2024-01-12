/*Takes a CUP file. Filter out the points by distance, keeping only the highest one in a given radius. Write a CUP file*/
/*Compilation: gcc cupFilterOutByDistance.c -lm -o cupFilterOutByDistance*/

/*cupFilterOutByDistance © 2024 by MLEP is licensed under CC BY 4.0 (https://creativecommons.org/licenses/by/4.0/?ref=chooser-v1)*/

/*TODO: debug (test for limit cases; memory leaks)*/
/*TODO: check for returns*/
/*TODO: process possible errors*/
/*TODO: beautify*/

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <limits.h>
#include <math.h>
#include <stdbool.h>

typedef signed short int OUTDATA_TYPE;
#define MAXLENGTHLINE 1000 /*maximum length for a line of characters*/

static char *version = "cupFilterOutByDistance - 2024-01-12";

char* getfield(char* line, int num){
  char* tok;
  for (tok = strtok(line, ","); tok && *tok; tok = strtok(NULL, ",\n")){
    if (!--num)
      return tok;
  }
  return NULL;
}/*getfield*/

/// @brief Count occurences of a given character in a string
/// @param s input string
/// @param letter the char to find
/// @return number of occurences of the given char within the string
int countChars(char *s, char c) {
    return *s == '\0'
              ? 0
              : countChars( s + 1, c ) + (*s == c);
}

int main(int argc, char *argv[]){
  char filename_in[FILENAME_MAX], filename_out[FILENAME_MAX];
  FILE *In, *Out;
  struct point{
    char line[MAXLENGTHLINE]; 
    double longitude;
    double latitude;
    double altitude;
	struct point* next;
  };

  struct point *begin; /*of the chain*/
  struct point *lastheader; /*last line in the header*/
  struct point *current;
  struct point *current_previous;
  struct point *moving;
  struct point *moving_previous;
  struct point *delete;
  struct point *lastpoint; /*last point in the chain*/

  char stringcoord[MAXLENGTHLINE]; 
  signed int degreeINT;
  double minutes;
  double longitude;
  double latitude;
  double altitude;
  char direction[MAXLENGTHLINE];
  double distance;
  double maxdistance;
  int header_length = 0;
  int footer_length = 0;
  int numberoflines;
  int numberofpoints;
  int i;
  int csvHeaderCommaCount = 0;
  bool isHeaderAutoDetection = (argc == 4) ? true : false;


  // Parse CLI arguments
  if(!isHeaderAutoDetection && argc != 6){
    fprintf(stderr, "\t%s\n", version);
    fprintf(stderr, "\tSyntax : %s input.cup dist_km output.cup <header_length footer_length> \n", argv[0]);
    exit(EXIT_FAILURE);
  }/*if*/

  if (!isHeaderAutoDetection) {
    header_length= atoi(argv[4]);
    footer_length= atoi(argv[5]);
  }
  strcpy(filename_in,  argv[1]);
  maxdistance= atof(argv[2]);
  strcpy(filename_out, argv[3]);
  
  // Open input and output files
  In=fopen(filename_in, "r");
  if(In==NULL){
    fprintf(stderr,"\n%s: problem opening the input file\n");
    exit(EXIT_FAILURE);
  }/*if*/

  Out=fopen(filename_out, "w");
  if(Out==NULL){
    fprintf(stderr,"\n%s: problem opening the output file\n");
    exit(EXIT_FAILURE);
  }/*if*/

  /*Initialize the chain of points*/
  current = (struct point*) malloc(sizeof(struct point));
  current->next=NULL;
  begin = current;

  /*read the CUP file*/
  numberoflines=0;
  while(fgets(current->line, sizeof(current->line), In)){
  current->next= (struct point*) malloc(sizeof(struct point));
  current=current->next;
  numberoflines++;
  }

  // Header and footer auto detection
  if (isHeaderAutoDetection) {
    current = begin;
    for(i=0; i<numberoflines; i++){
      if ( strstr(current->line, "name,code,country") != NULL || (strstr(current->line, "version=") != NULL) ) {
        csvHeaderCommaCount = countChars(current->line, ',');
      header_length++;
      }
      else {
        int lineCommaCount = countChars(current->line, ',');
        if (lineCommaCount != csvHeaderCommaCount) {            // We've found the first line that is not respecting the header format
          footer_length = numberoflines = i;
        }
      }
      current = current->next;
    }
  }
  
  numberofpoints=numberoflines-header_length-footer_length;

  /*set pointers before the first point and on last point*/
  lastheader=begin;
  for(i=0; i<header_length-1;i++){
    lastheader=lastheader->next;
  }
  lastpoint=lastheader;
  for(i=0; i<numberofpoints;i++){
    lastpoint=lastpoint->next;
  }

  /*for each point, stores longitude, latitude & altitude as decimals*/ 
  current=lastheader->next; /* = first point*/
  for(i=0; i<numberofpoints; i++){
    /*extract latitude*/
    strcpy(stringcoord,current->line);
    strcpy(stringcoord, getfield(stringcoord, 4));
    sscanf(stringcoord, "%lg%c", &latitude, &direction);
    if(strcmp("S", direction)==0){
       latitude=-latitude;
    }
    /*convert DDMM.MM into decimal degrees*/
    degreeINT=(int)(latitude/100);
    minutes=(latitude-100.0*(double)degreeINT);
    latitude=(double)degreeINT+minutes/60.0;
    current->latitude=latitude;

    /*extract longitude*/
    strcpy(stringcoord,current->line);
    strcpy(stringcoord, getfield(stringcoord, 5));
    sscanf(stringcoord, "%lg%c", &longitude, &direction);
    if(strcmp("W", direction)==0){
      longitude=-longitude;
    }
    /*convert DDMM.MM into decimal degrees*/
    degreeINT=(int)(longitude/100);
    minutes=(longitude-100.0*(double)degreeINT);
    longitude=(double)degreeINT+minutes/60.0;
    current->longitude=longitude;

    /*extract altitude*/
    strcpy(stringcoord,current->line);
    strcpy(stringcoord, getfield(stringcoord, 6));
    sscanf(stringcoord, "%lg", &altitude);
    current->altitude=altitude;

    current=current->next;
  }/*for*/

  /*scan the points: does one of the 2 points being compared need to be removed?*/
  current_previous=lastheader;
  current=lastheader->next; /* = first point of the list*/
  moving_previous=current;
  moving=current->next; /* = second point of the list*/
  while((current!=lastpoint)&&(current_previous!=lastpoint)){
    /*calculate distance*/
    distance=sqrt(pow(current->longitude - moving->longitude,2)+pow(current->latitude - moving->latitude,2)); /*in degrees*/
    distance=distance*111.0; /*in km  (1° = 111 km)*/
    if(distance<maxdistance){ 
      /*remove the lowest point*/
      if(moving->altitude < current->altitude){
        if(moving==lastpoint){
          lastpoint=moving_previous;
        }
        delete=moving;
        moving=moving->next;
        moving_previous->next=moving;
        free(delete);
      }else{/*start with a new current point*/
        delete=current;
        current=current->next;
        current_previous->next=current;
        free(delete);
        moving_previous=current;
        moving=current->next;
      }/*else*/
    }else{/*move to the following point*/
      moving_previous=moving;
      moving=moving->next;
    }/*else*/
    if(moving==lastpoint->next){ /*moving has reach the end of the list of points ; start over with a new current point*/
      current_previous=current;
      current=current->next;
      moving_previous=current;
      moving=current->next;
    }/*if*/
  }/*while*/

  /*write the CUP file*/
  current=begin;
  moving=begin;
  do{
    printf("%s", &current->line);
    fprintf(Out, "%s", &current->line);
    current=current->next;
    free(moving);
    moving=current;
  }while(current != NULL);

  (void)fclose(In);
  (void)fclose(Out);
  return EXIT_SUCCESS;
}/*main*/


