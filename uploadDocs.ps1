#
# uploads a documetation folder to the gh_branch
#
[CmdletBinding()]
Param
  (    
    [parameter(Mandatory=$true,Position=1)]
    [String]$SiteFolder,
    [parameter(Mandatory=$true,Position=2)]
    [String]$Project,
	[parameter(Mandatory=$true,Position=3)]
    [String]$User,
	[parameter(Mandatory=$true,Position=4)]
    [String]$EMail,	
	[parameter(Mandatory=$true,Position=5)]
    [String]$Token
  )

echo "init git"
git config --global credential.helper store
Add-Content "$HOME\.git-credentials" "https://$Token:x-oauth-basic@github.com`n"
git config --global user.email $EMail
git config --global user.name $User

$source=$pwd
$repro=[System.IO.Path]::GetFullPath("$pwd\..\$Project-Documentation")

echo "removing temporary doc directory $repro"
rm $repro -Force -Recurse -ErrorAction SilentlyContinue
mkdir $repro | Out-Null

$url = "https://github.com/$User/$Project.git"
echo "cloning the repo $url with the gh-pages branch"
git clone $url --branch gh-pages $repro

echo "clear repo directory"
cd $repro
git rm -r *

echo "copy documentation into the repo"
cp -r "$source\$SiteFolder\*" .

echo "push the new docs to the gh-pages branch"
git add . -A
git commit -m "update generated documentation"
git push origin gh-pages -q

cd $source