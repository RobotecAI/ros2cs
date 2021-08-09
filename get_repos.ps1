$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition

if (([string]::IsNullOrEmpty($Env:ROS_DISTRO)))
{
    Write-Host "Can't detect ROS2 version. Source your ros2 distro first." -ForegroundColor Red
    exit
}

$src_path = Join-Path -Path $scriptPath -ChildPath "\src"
$repos_file = Join-Path -Path $scriptPath -ChildPath "\ros2_$Env:ROS_DISTRO.repos"
$custom_repos_file = Join-Path -Path $scriptPath -ChildPath "\custom_messages.repos"
$repos_file = Resolve-Path -Path $repos_file
if (Test-Path -Path $repos_file) {
    Write-Host "Detected ROS2 $Env:ROS_DISTRO. Getting required repos from $repos_file" -ForegroundColor Green
    vcs import --input $repos_file $src_path
    if ($args[0] -eq "--get-custom-messages") {
        Write-Host "Getting custom messages from $custom_repos_file" -ForegroundColor Green
        vcs import --input $custom_repos_file $src_path
    }
} else {
    Write-Host "Can't find repos file : '$src_path' doesn't exist." -ForegroundColor Red
}
