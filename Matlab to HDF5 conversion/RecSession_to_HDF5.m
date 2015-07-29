
% Copyright 2015 Martin Garcia Carmueja 
% 
%  This file is part of the Myoelectric Personal Training and Control Environment (MPTCE).
%
%  MPTCE is free software: you can redistribute it and/or modify
%  it under the terms of the GNU General Public License as published by
%  the Free Software Foundation, either version 3 of the License, or
%  (at your option) any later version.
%
%  MPTCE is distributed in the hope that it will be useful,
%  but WITHOUT ANY WARRANTY; without even the implied warranty of
%  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
%  GNU General Public License for more details.
%
%  You should have received a copy of the GNU General Public License
%  along with MPTCE.  If not, see <http://www.gnu.org/licenses/>.
 


function RecSession_to_HDF5(filename,recSession)
% RecSession_to_HDF5: writes a BioPatRec recSession structure into an
% HDF5 file
%    filename: output file name. Extension must be specified.
%    recSession: name of the variable holding the recSession structure in
%    memory.
%
% The generated HDF5 file uses one data set with one subset. /recSession 
% holds the recording session parameters, and its subset tdata/ holds 
% the actual recording data

h5create(filename,'/recSession/tdata',size(recSession.tdata));%[36000 8 26]);
h5write(filename,'/recSession/tdata',recSession.tdata);

%Writing recording session parameters as attributes
h5writeatt(filename,'/recSession','sF',recSession.sF);
h5writeatt(filename,'/recSession','sT',recSession.sT);
h5writeatt(filename,'/recSession','cT',recSession.cT);
h5writeatt(filename,'/recSession','rT',recSession.rT);
h5writeatt(filename,'/recSession','nM',recSession.nM);
h5writeatt(filename,'/recSession','nR',recSession.nR);
h5writeatt(filename,'/recSession','nCh',recSession.nCh);
h5writeatt(filename,'/recSession','date',recSession.date);

%The list of movements is encodes as a semicolon-delimited string
result=[];
 for i=1:length(recSession.mov)
     result=[result char(recSession.mov(i))];
     if (i<length(recSession.mov))
         result = [result ';'];
     end
 end
 
h5writeatt(filename,'/recSession','mov',result);
h5writeatt(filename,'/recSession','dev',recSession.dev{1});
h5writeatt(filename,'/recSession','cmt',recSession.cmt{1});



