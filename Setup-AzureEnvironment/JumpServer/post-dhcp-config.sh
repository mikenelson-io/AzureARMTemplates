#!/bin/bash

echo supersede domain-name \" $1\"\; >> /etc/dhcp/dhclient.conf
service network restart
