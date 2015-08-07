#!/usr/bin/perl -w
sub traverse;

# generate a table of contents by concatenating all
# index.html files from sub directories one level deep
#
# run from the root directory of the documemation module
# redirect output to create table of contents

my @headers = qw(getting-started modeling configuration client-api storage security);

print <<EOD;
---
title: $ARGV[0]
layout: submenu
---
<h2>{{page.title}}</h2>
EOD

foreach my $header (@headers) {
  #read contents of index.html in sub directory
  open(my $fh, "$header/index.html") or die "can't open file $header/index.html: $!";
  my @lines = <$fh>;
  close $fh;
  splice(@lines, 0, 4);
  s/href="(.*?)"/href="$header\/$1"/ for @lines;
  print @lines;
}

#traverse ".", "  ";

# sub traverse {
#   my ($dir, $indent) = @_;
#
#   opendir(my $dh, $dir) || die "can't opendir $dir: $!";
#   my @dirs = grep {/^[a-z]/i && -d "$dir/$_" } readdir($dh);
#   closedir $dh;
#   return if @dirs == 0;
#   print "$indent<ol>\n";
#   for my $d(@dirs) {
#     my $pretty = $d;
#     $pretty =~ s/-/ /g;
#     $pretty =~ s/(^[a-z]| [a-z])/uc($1)/eg;
#     print "$indent$indent<li><a href='$dir/$d'>$pretty</a>\n";
#     traverse "$dir/$d", $indent . "  ";
#     print "$indent$indent</li>\n";
#   }
#   print "$indent</ol>\n";
#
# }
