How to use ConstantsGenerator:

** Prerequisite
Make sure your GOLD Parser Builder installation has the "Epi Info.pgt" template file in its Templates folder.

** Steps
1.  Compile the contents of EpiInfoGrammar.txt using GOLD Parser Builder to generate a CGT file.
2.  Click on "Tools -> Create Skeleton Program" in GOLD Parser Builder.
3.  Choose "Epi Info" as the template.
4.  Click "Create" to generate an XML file.
6.  Run ConstantsGenerator.
7.  Click "Generate Constants".
8.  Select the XML file generated in Step 4 to generate the constants.

    *** The constants have now been generated***

9.  Replace the source controlled "EpiInfoGrammar.cgt" file with the one generated in Step 1.
10. Replace the contents of "ParserConstants.cs" with the constants generated in Step 8.