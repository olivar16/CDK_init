# CDK_init
C# program produced during Summer 2015 Internship at CDK Global <br>

Background: ADPinit is a program that is installed on Client PCs to ensure that the proper user environment is set up for each user that logs on. The program is table-structured and abstracted in such a way that all necessary configurations can be made in the config.xml file. Program functions include: <br>
1. Setting appropriate Registry values for the environment <br>
2. Modifying values in the environment .Ini file <br>
3. Ensuring that files exist in the given paths of the user's machine. If not, then a back up is copied to that location from the Installation Directory. <br>
