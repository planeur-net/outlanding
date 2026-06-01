#!/usr/bin/perl
#
# getInfosFromApiBasulm.pl
# 
# recuperation des infos BASULM, a partir de l'API specifique
# genere le fichier listULMfromAPI.csv, qui sera utilise par d'autres moulinettes
#
# ATTENTION : l'altitude est exprimee en pieds
#
# Il faut au pr�alable demander une cle d'authentification a admin.basulm@orange.fr
#   Elle est ensuite accessible sur le site ffplum (https://basulm.ffplum.fr). cliquer sur MYBASULM
#
# doc de l'API basulm : https://basulm.ffplum.fr/mode-d-emploi-api.html
#
# pour essais de l'API, avec curl :
#    curl -H "Authorization: api_key <api_key>" "https://basulm.ffplum.fr/getbasulm/get/basulm/liste"
#    curl -H "Authorization: api_key <api_key>" "https:basulm.ffplum.fr/getbasulm/get/basulm/detail?id=5118"
#
#
# genere en sous-produit le resultat de la requete API brute, en format JSON : c'est le fichier basulm.json
#
# si ex�cut� sans parametre, donne de l'aide
#
# ne traite que les terrains de la m�tropole : n'accepte que des terrais en format LFxxxx ou LFww
#
# parametres accept�s :
#  . -key <api-key>. facultatif. C'est la cl� API d'interrogation BASULM
#  . --replay : facultatif. Si present, permet de rejouer ce programme � partir du fichier basulm.json, sans interroger directement BASULM via l'API
#  . --verbose : facultatif. Si present, genere un message lors de MaJ de l'altitude
#  . --help : facultatif. Affiche une  aide
#
# Un des deux param�tres '-key' ou '--replay' doivent �tre pr�sents
#
# Exemple d'ex�cution :
# getInfosFromApiBasulm.pl -key xxxxxxxx


use lib ".";       # necessaire avec strawberry, pour VAC.pm
use VAC;
use JSON;
use Getopt::Long qw(:config no_ignore_case no_auto_abbrev);
use Text::Unaccent::PurePerl qw(unac_string);
use FindBin;

use Data::Dumper;

use strict;

my $baseURL = "https://basulm.ffplum.fr/getbasulm/get/basulm";  # url d'acces a l'API BASULM
my $listURL = "$baseURL/listall";                              # la liste des terrains. infos detailles
#my $listURL = "$baseURL/liste";                                # la liste des terrains. infos simplifiees. Pour essais
#my $listURL = "$baseURL/detail?id=10850";                      # detail d'un terrai. Pour essai
my $ficOUT = "listULMfromAPI.csv";                              # le fichier resultant


# des terrains nomenclatures dans les bases SIA ou BASULM, qu'on ne d�sire pas traiter
my $noADs = $VAC::noADs;

my $ficReference = "$FindBin::Bin/FranceVacEtUlm.cup";  # ce fichier va permettre d'ajouter l'information d'altitude pour ceux qui n'en ont pas, que QFU pour LF1757

my $verbose; 

binmode(STDOUT, ":utf8");

{
  my ($api_key, $replay, $help);
  
    my $ret = GetOptions
     ( 
       "key=s"          => \$api_key,
	   "replay"         => \$replay,
	   "v|verbose"      => \$verbose,
	   "h|help"         => \$help,
	 );
  
  die "parametre incorrect" unless($ret);
  &syntaxe() if ($help);
  &syntaxe() if (! defined($api_key) && ! defined($replay));

  my $page;   # va contenir le resultat de l'interrogation BASULM, en format JSON
  if ($api_key ne "")
  {
    my ($code, $cookies);
    ($code, $page, $cookies) = &sendHttpRequest($listURL, SSL_NO_VERIFY => 1, AUTHORIZATION => "api_key $api_key");
    die "Impossible de charger la page $listURL" unless (defined($page));
	#print "$page\n"; exit;
    { local(*OUTPUT, $/); open (OUTPUT, ">", "basulm.json") || die "can't open basulm.json"; print OUTPUT $page; close OUTPUT }; # ecrire resultat dans fichier
  }
  
  if (defined($replay))
    { local(*INPUT, $/); open (INPUT, "basulm.json") || die "can't open basulm.json"; $page = <INPUT>; close INPUT };  # pour lire le fichier basulm.json  

  my $baseULMs = &decodeInfosBASULM(\$page);  # decode les infos JSON recues de l'API BASULM
  #print Dumper($$baseULMs{LF4161});
  
  my $ULMs = &recoupeInfos($baseULMs, $ficReference);  # on recoupe les infos baseULM avec le fichier .CUP de reference
  #print Dumper($$ULMs{LF4161}); exit;
  
   die "unable to write fic $ficOUT" unless (open (FICOUT, ">:utf8", $ficOUT));
  foreach my $code (sort keys %$ULMs)
  {
    my $ULM = $$ULMs{$code};
    print FICOUT "$code;basulm;$$ULM{name};$$ULM{lat};$$ULM{lon};$$ULM{elev};$$ULM{nature};$$ULM{rwdir};$$ULM{rwlen};$$ULM{rwwidth};$$ULM{freq};BASULM $$ULM{type}\n";
  }
  close FICOUT;
}

# recoupe les infos recues de BASULM avec les infos du fichier de reference  
sub recoupeInfos
{
  my $baseULMs = shift;
  my $ficRef = shift;
  
  my %natures = (  1 => "eau", 2 => "herbe", 3 => "neige", 5 => "dur" );  # codage dans fichier .CUP de ref
  
  my $ADRefs = &readCupFile($ficRef, ref => 1);    # infos de reference
  
  my %ULMs = ();      # le hash r�sultant
  
  foreach my $code (sort keys %$baseULMs)
  {
    #next if ($code ne "LF0158");
    my $AD = $$baseULMs{$code};    # C'est l'info brute, recuperee par l'API baseULM
	my $ADRef = $$ADRefs{$code};
	#print Dumper($ADRef), Dumper($AD); exit;
	my $name = $$AD{name};		
	my $type = $$AD{type};
	$type = unac_string($type);
	$type =~ s/ $//;
    next if ($type =~ /ferme.? temporairement/i);
	next if ($type =~ /ferme.? definitivement/i);
	next if ($type =~ /temporairement ferme.?/i);
	next if ($type =~ /aerodrome ferme/i);
	next if ($type =~ /terrain mal defini/i);
	
	if ((! defined($ADRef)) && (! defined($$noADs{$code})))
	{
	  if ($$AD{elev} ne "")
	  {
	    print "WARNING. $code;$name;$$AD{lat};$$AD{lon};$$AD{elev} . Ne se trouve pas dans la base de reference\n";
	  }
	  else
	  {
	    print "WARNING. $code;$name;$$AD{lat};$$AD{lon}. Ne se trouve pas dans la base de reference. Pas d'altitude indiquee\n";
	  }
	}	
	
	my $infos = {name => $name, type => $type, code => $code, rwlen => $$AD{rwlen}, rwwidth => $$AD{rwwidth}};

	my $lat = $$AD{lat};
	my $long = $$AD{lon};
	$lat =~ s/^N (.*)/\1 N/;
	$long =~ s/^([EW]) (.*)/\2 \1/;
    $$infos{lat} = $lat;
	$$infos{lon} = $long;

	my $nature = $$AD{nature};
	$nature = "herbe" if ($nature eq "terre");
	#die "nature de terrain pas trouve : $code;$name;$nature\n" 
	if (($nature ne "dur") && ($nature ne "herbe") && ($nature ne "neige")&& ($nature ne "eau"))
	{                               # il n'y en a pas normalement
	  my $numNat = $$ADRef{nature};
	  if (defined($numNat))    
	  {
	    $nature = $natures{$numNat};
	    print "$code;$name. nature de terrain |$nature| recuperee du fichier de reference\n";
      }
	  else
	  {
	    die "$code;$name. Nature de terrain inconnue\n";
	  }
    }
	$$infos{nature} = $nature;
	
	my $elev = $$AD{elev};
	if ($elev =~ /(\d+) ft/)
	{
	  $elev = $1;
	}
	else
	{
	  if ($elev eq "")   # pas d'altitude dans basulm. On prend celle du ficheir de ref
	  {
	    $elev = $$ADRef{elev};
	    if (defined($elev))   # il y en a plusieurs
	    {
	      print "$code;$name. Altitude |${elev}m| recuperee du fichier de reference\n" if ($verbose);
        }
	    else
	    {
	      print "WARNING. $code;$name. Altitude inconnue\n" if ($verbose);
	    }
	  }
	}
	$$infos{elev} = $elev if ($elev ne "");
	
	my $rwdir = $$AD{rwdir};   # orientation preferee
	if ($rwdir =~ /^ *\d\d?$/)  
	{  
      $$infos{rwdir} = $rwdir . "0"  # car qfu dans basulm, et degres dans .cup
	}
	else
	{
	  if (($$AD{rwdir} ne "omnidir") && ($$AD{rwdir} ne "Inconnue"))
	  {
	    print Dumper($ADRef), Dumper($AD), Dumper($infos);
	    die "Impossible recuperer rwdir : $name;$$AD{rwdir}";
	  }
	}
	
	my $freq = $$AD{freq};
	$freq =~ s/,/\./g;
	if (($freq ne "") && ($freq =~ /([\d\.]+)/))
	{
	  $freq = $1;
	  $$infos{freq} = $freq;
	}
	
	$ULMs{$code} = $infos;
  }
  return \%ULMs;
}

###### decodage des infos JSON retourn�es par l'API BASULM ##################
sub decodeInfosBASULM
{
  my $page = shift;
  
  my %baseULM = ();
  my @infosRequired = ("name", "elev", "lat", "lon", "rwlen", "rwwidth", "nature", "type");
      
	my $json;
	eval { $json = decode_json($$page); 1 } or do {
		my $sample = $$page;
		$sample =~ s/[\r\n]+/ /g;
		$sample = substr($sample, 0, 200);
		my $hint = "";
		if ($sample =~ /LWP will support https/i)
		{
			$hint = "\nIndice: le support HTTPS de LWP est indisponible (Net::SSLeay/IO::Socket::SSL).";
		}
		die "la r�ponse BASULM n'est pas en format JSON. Debut de reponse: '$sample'.$hint\nVoir fichier basulm.json";
	};
  
  if ($$json{status} ne "ok")
  {
    print STDERR "Erreur dans la reponse BASULM : \n";
	print STDERR "code : $$json{error_code}, detail : $$json{error_description}\n";
	exit(1);
  }
    
  my $liste = $$json{liste};
  
  foreach my $AD (@$liste)
  {
	my $code = $$AD{code_terrain};
	next if ($code eq "");
	next if ($code !~ /^LF\d\d\d\d$/ && $code !~ /^LF\w\w$/);
	#next if ($code ne "LF0121");

	my $infos = {};
	$$infos{code} = $code;
	
	my $name = $$AD{toponyme};
	next if ($name eq "");
	$name =~ s/ $//;  # on retire un eventuel espace en fin
	$$infos{name} = $name;
	$$infos{type} = $$AD{type_terrain};
    $$infos{lat} = $$AD{latitude};
	$$infos{lon} = $$AD{longitude};
	$$infos{elev} = $$AD{altitude};
	$$infos{nature} = $$AD{nature_piste_1};
	$$infos{rwlen} = $$AD{longueur_piste_1};
	$$infos{rwwidth} = $$AD{largeur_piste_1};
	$$infos{freq} = $$AD{radio};

    my $rwdir;
    #$rwdir = $$AD{orientation_pref_1} unless ($$AD{orientation_pref_1} =~ /^\d\d\d?/);  # ex : LF6555 : "orientation_pref_1": "300"
    $rwdir = $$AD{orientation_pref_1} unless ($$AD{orientation_pref_1} > 99);  # ex : LF6555 : "orientation_pref_1": "300"
    $rwdir = $$AD{orientation_piste_1} if (($rwdir eq "") && ($$AD{orientation_piste_1} ne ""));
    $rwdir =~ s/^0(\d\d)/$1/;      # ex : LF3765 : "orientation_pref_1" = "015"	
	$rwdir = $1 if ($rwdir =~ /^(\d\d)-/);
	$$infos{rwdir} = $rwdir;
	#print "$code. pref : $$AD{orientation_pref_1} . piste : $$AD{orientation_piste_1} . rwdir : $rwdir\n";
	#print Dumper($AD), Dumper($infos); exit;
	
	foreach my $infosRequired (@infosRequired)
    {
	  unless (defined($$infos{$infosRequired}))
	  {	
	    print "$code. Impossible de recuperer l'info \"$infosRequired\" pour le terrain $code\n";
	    print "Arret du programme\n";
	    print Dumper($infos);
	    exit 1;
	  }
	}

	$baseULM{$code} = $infos;
  }
  return \%baseULM;
}

sub syntaxe
{
  print "getInfosFromApiBasulm.pl\n";
  print "Ce script permet d'interroger les donnees BASULM a l'aide de son API. G�n�re le fichier listULMfromAPI.csv\n";
  print "G�n�re en sous-produit le fichier basulm.json qui est le r�sultat brut de l'interrogation BASULM\n\n";
  print "les parametres sont :\n";
  print "  . -key <api-key>. facultatif. C'est la cl� API d'interrogation BASULM\n";
  print "  . --replay : facultatif. Si present, permet de rejouer ce programme � partir du fichier basulm.json, sans interroger directement BASULM via l'API\n";
  print "  . --verbose : facultatif. Si present, genere un message lors de MaJ de l'altitude\n";
  print "  . --help : facultatif. Affiche cette aide\n\n";
  print "Un des deux param�tres '-key' ou '--replay' doivent �tre pr�sents\n";
  print "\n";
  
  exit;
}